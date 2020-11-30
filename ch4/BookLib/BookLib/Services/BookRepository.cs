using System;
using System.Collections.Generic;
using System.Linq;
using BookLib.Models;
using Library.API.Data;

namespace BookLib.Services
{
    public class BookRepository: IBookRepository
    {
        public BookRepository()
        {
        }

        public BookDto GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return LibraryMockData.Current.Books.FirstOrDefault(b => b.AuthorId == authorId && b.Id == bookId);
        }

        public IEnumerable<BookDto> GetBooksForAuthor(Guid authorId)
        {
            return LibraryMockData.Current.Books.Where(b => b.AuthorId == authorId).ToList();
        }
    }
}
