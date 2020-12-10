using System;
using BookLib.Entities;

namespace BookLib.Services
{
    //包装器的实现，提供了对所有仓储接口的统一访问方式，避免单独访问每个仓储接口
    public class RepositoryWrapper: IRepositoryWrapper
    {
        private IAuthorRepository _authorRepository = null;
        private IBookRepository _bookRepository = null;

        public LibraryDbContext LibraryDbContext { get; }
        public RepositoryWrapper(LibraryDbContext  libraryDbContext)
        {
            LibraryDbContext = libraryDbContext;
        }

        public IBookRepository Book => _bookRepository ?? new BookRepository(LibraryDbContext);
   
        public IAuthorRepository Author => _authorRepository ?? new AuthorRepository(LibraryDbContext);

    }
}
