using System;
using System.Threading.Tasks;
using BookLib.Entities;
using BookLib.Helpers;

namespace BookLib.Services
{
    public interface IAuthorRepository:IRepositoryBase<Author>, IRepositoryBase2<Author,Guid>
    {
        Task<PagedList<Author>> GetAllAsync(AuthorResourceParameters parameters);
    }
}
