using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookLib.Entities;
using BookLib.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using BookLib.Extensions;

namespace BookLib.Services
{
    public class AuthorRepository: RepositoryBase<Author, Guid>, IAuthorRepository
    {
        private Dictionary<string, PropertyMapping> mappingDict = null;
        public AuthorRepository(DbContext dbContext): base(dbContext)
        {
            mappingDict = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);
            mappingDict.Add("Name", new PropertyMapping("Name"));
            mappingDict.Add("Age", new PropertyMapping("BirthDate", true));
            mappingDict.Add("BirthPlace", new PropertyMapping("BirthPlace"));
        }

        //IAuthorRepository 里面定义针对Author Repository的GetAllAsync方法
        public Task<PagedList<Author>> GetAllAsync(AuthorResourceParameters parameters)
        {
            IQueryable<Author> queryableAuthors = DbContext.Set<Author>();

            //过滤逻辑
            //加了一个birthplace 作为解构过滤条件
            if (!string.IsNullOrWhiteSpace(parameters.BirthPlace))
            {
                queryableAuthors = queryableAuthors.Where(m => m.BirthPlace.ToLower() == parameters.BirthPlace);
            }
            //搜索逻辑
            if (!string.IsNullOrWhiteSpace(parameters.SearchQuery))
            {
                queryableAuthors = queryableAuthors.Where(m => m.BirthPlace.ToLower().Contains(parameters.SearchQuery.ToLower())
                    || m.Name.ToLower().Contains(parameters.SearchQuery.ToLower()));
            }
            //此处SuperSort是IQueryable的扩展方法，其中解析了AuthorResourceParameters 中的SortBy属性，从而利用Dynamic Linq得到正确结果
            var orderedAuthors = queryableAuthors.SuperSort(parameters.SortBy, mappingDict);
            //CreateAsync是PagedList<T>定义的静态类，用来返回一个PagedList实例
            //分页逻辑就在CreateAsync方法里
            return PagedList<Author>.CreateAsync(orderedAuthors, parameters.PageNumber, parameters.PageSize);
        }
    }
}
