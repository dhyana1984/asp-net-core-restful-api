using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookLib.Entities;
using BookLib.Filter;
using BookLib.Models;
using BookLib.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BookLib.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ServiceFilter(typeof(CheckAuthorExistFilterAttribute))] //用此filter必须现在Startup注册到ConfigService中
    public class BookController : ControllerBase
    {
        public IRepositoryWrapper RepositoryWrapper { get; }
        public IMapper Mapper { get; }
        public IMemoryCache MemoryCache { get; }

        public BookController(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            RepositoryWrapper = repositoryWrapper;
            Mapper = mapper;
            MemoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookDto>>> GetBooksAsync(Guid authorId)
        {
            var bookDtoList = new List<BookDto>();
            string key = $"{authorId}_books";
            //内存缓存实际上是一个键值对字典
            //从内存缓存取数据，如果内存缓存中有该键，则直接赋值给bookDtoList并且返回
            if (!MemoryCache.TryGetValue(key, out bookDtoList))
            {
                var books = await RepositoryWrapper.Book.GetBooksAsync(authorId);
                bookDtoList = Mapper.Map<IEnumerable<BookDto>>(books).ToList();
                //如果内存缓存没有数据则保存进内存缓存
                //MemoryCacheEntryOptions 是用来配置该缓存entry
                //Priority是内存缓存被清除的优先级
                //AbsoluteExpirationRelativeToNow是缓存相对于当前时间经过多长时间过期
                //AbsoluteExpiration 是从上一次访问算起经过多长时间过期
                MemoryCache.Set(key, bookDtoList,new MemoryCacheEntryOptions { Priority=CacheItemPriority.NeverRemove, AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
                Console.WriteLine("from database");
            }
            else
            {
                Console.WriteLine("from memory cache");
            }

            return bookDtoList;
        }

        [HttpGet("{bookId}", Name = nameof(GetBookAsync))]
        public async Task<ActionResult<BookDto>> GetBookAsync(Guid authorId, Guid bookId)
        {
            var targetBook = await RepositoryWrapper.Book.GetBookAsync(authorId, bookId);
            if (targetBook == null)
            {
                return NotFound();
            }

            return Mapper.Map<BookDto>(targetBook);
        }

        [HttpPost]
        public async Task<IActionResult> AddBookAsync(Guid authorId, [FromBody] BookForCreationDto bookForCreationDto)
        {
            var newBook = Mapper.Map<Book>(bookForCreationDto);
            newBook.AuthorId = authorId;
            RepositoryWrapper.Book.Create(newBook);
            if(!await RepositoryWrapper.Book.SaveAsync())
            {
                throw new Exception("Create resource book failed!");
            }
            var bookDto = Mapper.Map<BookDto>(newBook);
            return CreatedAtRoute(nameof(GetBookAsync), new { authorId = authorId, bookId = newBook.Id }, bookDto);
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteBookAsync(Guid authorId, Guid bookId)
        {
            var book = await RepositoryWrapper.Book.GetBookAsync(authorId, bookId);
            RepositoryWrapper.Book.Delete(book);
            if (!await RepositoryWrapper.Book.SaveAsync())
            {
                throw new Exception("Delete resource book failed!");
            }
            return NoContent();//返回204
        }

        [HttpPut("{bookId}")] 
        public async Task<IActionResult> UpdateBookAsync(Guid authorId, Guid bookId, [FromBody] BookForUpdateDto updatedBook)
        {
            var book = await RepositoryWrapper.Book.GetBookAsync(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }
            //这个重载能将updatedBook map到已经存在的book实体
            Mapper.Map(updatedBook, book, typeof(BookForUpdateDto), typeof(Book));
            if (!await RepositoryWrapper.Book.SaveAsync())
            {
                throw new Exception("Update resource book failed!");
            }
            return NoContent();
        }

        //JsonPatchDocument<BookForUpdateDto> patchDocument 是JSON Patch文档格式，用HttpPatch请求的时候搭配之用实现部分更新资源
        [HttpPatch("{bookId}")]
        public async Task<IActionResult> PartiallyUpdateBookAsync(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            var book = RepositoryWrapper.Book.GetBookAsync(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }
            //现将原来的book实体映射到BookForUpdateDto对象
            var bookUpdateDto = Mapper.Map<BookForUpdateDto>(book);

            //ApplyTo是将patchDocument中相应的修改操作应用到新建的对象bookToPatch上
            //错误会被记录到到ModelStateDictionary中，通过ModelState.IsValid判断是否有错误，并且在错误时返回bad request 400
            patchDocument.ApplyTo(bookUpdateDto, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(bookUpdateDto, book, typeof(BookForUpdateDto), typeof(Book));
            if (!await RepositoryWrapper.Book.SaveAsync())
            {
                throw new Exception("Update resource book failed!");
            }
            return NoContent();

        }
    }
}
