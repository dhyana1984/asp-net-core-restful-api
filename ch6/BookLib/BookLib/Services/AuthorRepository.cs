using System;
using System.Linq;
using System.Threading.Tasks;
using BookLib.Entities;
using BookLib.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BookLib.Services
{
    public class AuthorRepository: RepositoryBase<Author, Guid>, IAuthorRepository
    {
        public AuthorRepository(DbContext dbContext): base(dbContext)
        {

        }

        //IAuthorRepository 里面定义针对Author Repository的GetAllAsync方法
        public Task<PagedList<Author>> GetAllAsync(AuthorResourceParameters parameters)
        {
            IQueryable<Author> queryableAuthors = DbContext.Set<Author>();
            //CreateAsync是PagedList<T>定义的静态类，用来返回一个PagedList实例
            //分页逻辑就在CreateAsync方法里
            return PagedList<Author>.CreateAsync(queryableAuthors, parameters.PageNumber, parameters.PageSize);
        }
    }
}
