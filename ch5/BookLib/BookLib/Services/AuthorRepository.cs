using System;
using BookLib.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLib.Services
{
    public class AuthorRepository: RepositoryBase<Author, Guid>, IAuthorRepository
    {
        public AuthorRepository(DbContext dbContext): base(dbContext)
        {

        }
    }
}
