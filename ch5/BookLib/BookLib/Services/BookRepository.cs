using System;
using BookLib.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLib.Services
{
    public class BookRepository: RepositoryBase<Book, Guid>, IBookRepository
    {
        public BookRepository(DbContext dbContext) : base(dbContext)
        {

        }
    }
}
