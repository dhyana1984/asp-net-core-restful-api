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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookLib.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ServiceFilter(typeof(CheckAuthorExistFilterAttribute))] //用此filter必须现在Startup注册到ConfigService中
    public class BookController : ControllerBase
    {
        public IRepositoryWrapper RepositoryWrapper { get; }
        public IMapper Mapper { get; }


        public BookController(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            RepositoryWrapper = repositoryWrapper;
            Mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookDto>>> GetBooksAsync(Guid authorId)
        {
            var books = await RepositoryWrapper.Book.GetBooksAsync(authorId);
            var bookDtoList = Mapper.Map<IEnumerable<BookDto>>(books);
            return bookDtoList.ToList();
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
