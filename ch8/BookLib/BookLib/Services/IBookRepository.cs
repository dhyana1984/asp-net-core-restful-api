using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLib.Entities;
using BookLib.Models;

namespace BookLib.Services
{
    public interface IBookRepository: IRepositoryBase<Book>, IRepositoryBase2<Book, Guid>
    {
        //在此定义的方法是IBookRepository自有的方法，需要单独实现
        Task<IEnumerable<Book>> GetBooksAsync(Guid authorId);
        Task<Book> GetBookAsync(Guid authorId, Guid bookId);
    }
}
