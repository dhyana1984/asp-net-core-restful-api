using System;
using System.Collections.Generic;
using BookLib.Models;

namespace BookLib.Services
{
    public interface IAuthorRepository
    {
        IEnumerable<AuthorDto> GetAuthors();
        AuthorDto GetAuthor(Guid authorId);
        bool IsAuthorExists(Guid authorId);
    }
}
