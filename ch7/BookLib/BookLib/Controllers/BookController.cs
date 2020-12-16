using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookLib.Entities;
using BookLib.Filter;
using BookLib.Models;
using BookLib.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;

namespace BookLib.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ServiceFilter(typeof(CheckAuthorExistFilterAttribute))] //用此filter必须现在Startup注册到ConfigService中
    [ApiController]
    public class BookController : ControllerBase
    {
        public IRepositoryWrapper RepositoryWrapper { get; }
        public IMapper Mapper { get; }
        public IMemoryCache MemoryCache { get; }
        public IHashFactory HashFactory { get; }

        public BookController(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IMemoryCache memoryCache,
            IHashFactory hashFactory)
        {
            RepositoryWrapper = repositoryWrapper;
            Mapper = mapper;
            MemoryCache = memoryCache;
            HashFactory = hashFactory;
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
            string entityHash = HashFactory.GetHash(targetBook);
            Response.Headers[HeaderNames.ETag] = entityHash;
            if (Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var requestETag) && requestETag == entityHash)
            {
                return StatusCode(StatusCodes.Status304NotModified);
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
        [CheckIfMatchHeaderFilter]
        public async Task<IActionResult> UpdateBookAsync(Guid authorId, Guid bookId, [FromBody] BookForUpdateDto updatedBook)
        {
            var book = await RepositoryWrapper.Book.GetBookAsync(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }
            //判断request中的if-match的etag是否和当前资源的Etag一致，如果不一致的话就返回412，PreconditionFailed,需要客户端重新取一下资源，更新etag
            //解决了一个请求修改了Book，但是另一个请求直接再次修改Book，这时第二个请求Etag和资源序列化后散列值不匹配，所以返回412状态码，必须先get最新的资源更新etag后再更新资源
            var entityHash = HashFactory.GetHash(book);
            if (Request.Headers.TryGetValue(HeaderNames.IfMatch, out var requestETag) && requestETag != entityHash)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            //request中的if-match的etag是否和当前资源的Etag一致就更新资源
            Mapper.Map(updatedBook, book, typeof(BookForUpdateDto), typeof(Book));
            RepositoryWrapper.Book.Update(book);
            if (!await RepositoryWrapper.Book.SaveAsync())
            {
                throw new Exception("Update resource book failed!");
            }

            //在Response的header中更新etag
            var entityNewHash = HashFactory.GetHash(book);
            Response.Headers[HeaderNames.ETag] = entityNewHash;
            return NoContent();
        }

        //JsonPatchDocument<BookForUpdateDto> patchDocument 是JSON Patch文档格式，用HttpPatch请求的时候搭配之用实现部分更新资源
        [HttpPatch("{bookId}")]
        [CheckIfMatchHeaderFilter]
        public async Task<IActionResult> PartiallyUpdateBookAsync(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            var book = RepositoryWrapper.Book.GetBookAsync(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }
            var entityHash = HashFactory.GetHash(book);
            if (Request.Headers.TryGetValue(HeaderNames.IfMatch, out var requestETag) && requestETag != entityHash)
            {
                return StatusCode(StatusCodes.Status412PreconditionFailed);
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
            var entityNewHash = HashFactory.GetHash(book);
            Response.Headers[HeaderNames.ETag] = entityNewHash;
            return NoContent();

        }
    }
}
