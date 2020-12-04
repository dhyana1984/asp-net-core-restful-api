using System;
using System.Collections.Generic;
using System.Linq;
using BookLib.Models;
using BookLib.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookLib.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BookController : ControllerBase
    {
        public IAuthorRepository AuthorRepository { get; }
        public IBookRepository BookRepository { get; }


        public BookController(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            AuthorRepository = authorRepository;
            BookRepository = bookRepository;
        }

        [HttpGet]
        public ActionResult<List<BookDto>> GetBooks(Guid authorId)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            return BookRepository.GetBooksForAuthor(authorId).ToList();
        }

        [HttpGet("{bookId}", Name = nameof(GetBook))]
        public ActionResult<BookDto> GetBook(Guid authorId, Guid bookId)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            var targetBook = BookRepository.GetBookForAuthor(authorId, bookId);
            if (targetBook == null)
            {
                return NotFound();
            }
            
            return targetBook;
        }

        [HttpPost]
        //BookForCreationDto bookForCreationDto需要加[FromBody], authorId是从url传进来的
        public IActionResult AddBook(Guid authorId, [FromBody] BookForCreationDto bookForCreationDto)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            var newBook = new BookDto
            {
                Title = bookForCreationDto.Title,
                Description = bookForCreationDto.Description,
                Pages = bookForCreationDto.Pages,
                AuthorId = authorId
            };

            BookRepository.AddBook(newBook);
            return CreatedAtRoute(nameof(GetBook), new { authorId = authorId, bookId = newBook.Id }, newBook);
        }

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBook(Guid authorId, Guid bookId)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            var book = BookRepository.GetBookForAuthor(authorId, bookId);

            BookRepository.DeleteBook(book);
            return NoContent();//返回204
        }

        [HttpPut("{bookId}")] //如果用put更新资源，在body里面没有定义的属性会被更新为默认值
        public IActionResult UpdateBook(Guid authorId, Guid bookId, [FromBody] BookForUpdateDto updatedBook)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            var book = BookRepository.GetBookForAuthor(authorId, bookId);

            if(book == null)
            {
                return NotFound();
            }

            BookRepository.UpdateBook(authorId, bookId, updatedBook);
            return NoContent();
        }

        //JsonPatchDocument<BookForUpdateDto> patchDocument 是JSON Patch文档格式，用HttpPatch请求的时候搭配之用实现部分更新资源
        [HttpPatch("{bookId}")]
        public IActionResult PartiallyUpdateBook(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (!AuthorRepository.IsAuthorExists(authorId))
            {
                return NotFound();
            }

            var book = BookRepository.GetBookForAuthor(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }

            var bookToPatch = new BookForUpdateDto
            {
                Title = book.Title,
                Description = book.Description,
                Pages = book.Pages
            };
            
            //ApplyTo是将patchDocument中相应的修改操作应用到新建的对象bookToPatch上
            //错误会被记录到到ModelStateDictionary中，通过ModelState.IsValid判断是否有错误，并且在错误时返回bad request 400
            patchDocument.ApplyTo(bookToPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Console.WriteLine(bookToPatch.Title);
            BookRepository.UpdateBook(authorId, bookId, bookToPatch);
            return NoContent();

        }
    }
}
