using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLib.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLib.Services
{
    public class BookRepository: RepositoryBase<Book, Guid>, IBookRepository
    {
        public BookRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Book> GetBookAsync(Guid authorId, Guid bookId)
        {
            return await DbContext.Set<Book>().SingleOrDefaultAsync(book => book.AuthorId == authorId && book.Id == bookId);
        }

        public Task<IEnumerable<Book>> GetBooksAsync(Guid authorId)
        {
            return Task.FromResult(DbContext.Set<Book>().Where(book => book.AuthorId == authorId).AsEnumerable());
        }
    }
}
