using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookLib.Entities;
using BookLib.Helpers;
using BookLib.Models;
using BookLib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace BookLib.Controllers
{
    [Route("api/authors")] //指定该controller的路由地址
    [ApiController] //该属性是在core2.1中添加的特性，有些方便功能，比如自动模型验证，action参数来源推断等
    [Authorize]
    public class AuthorController : ControllerBase
    {
        public IRepositoryWrapper RepositoryWrapper { get; }
        public IMapper Mapper { get; }
        public ILogger<AuthorController> Logger { get; }
        public IHashFactory HashFactory { get; }

        public AuthorController(
                IRepositoryWrapper repositoryWrapper,
                IMapper mapper,
                ILogger<AuthorController> logger,
                IHashFactory hashFactory)
        {
            RepositoryWrapper = repositoryWrapper;
            Mapper = mapper;
            Logger = logger;
            HashFactory = hashFactory;
        }

        [HttpGet(Name = nameof(GetAuthorsAsync))] // nameof(GetAuthorsAsync)一定不能漏，否则rl.Link(nameof(GetAuthorsAsync)会返回null
        [ResponseCache(CacheProfileName = "Default", VaryByQueryKeys = new string[] {"sortBy","searchQuery" })] //VaryByQueryKeys能够区别不同query string重复发送request时是否从缓存响应
        public async Task<ActionResult<ResourceCollection<AuthorDto>>> GetAuthorsAsync([FromQuery] AuthorResourceParameters parameters)//页码信息从url中取，因为不是resource数据
        {
            //throw new Exception("test exception filter");
            var pagedList = await RepositoryWrapper.Author.GetAllAsync(parameters);
            //Logger.LogWarning(1, parameters.SortBy); //Add log
            //匿名对象，包括所有分页的有关信息
            var paginationMetadata = new
            {
                //记录总数
                totalCount = pagedList.TotalCount,
                //每页记录数
                pageSize = pagedList.PageSize,
                //当前页
                currentPage = pagedList.CurrentPage,
                //所有页数
                totalPages = pagedList.TotalPages,
                //上一页的链接,Url.Link方法可以根据路由生成url
                previousePageLink = pagedList.HasPrevious ? Url.Link(nameof(GetAuthorsAsync), new
                {
                    pageNumber = pagedList.CurrentPage - 1,
                    pageSize = pagedList.PageSize,
                    birthPlace = parameters.BirthPlace,
                    searchQuery = parameters.SearchQuery,
                    sortBy = parameters.SortBy
                }) : null,
                //下一页的链接
                nextPageLink = pagedList.HasNext ? Url.Link(nameof(GetAuthorsAsync), new
                {
                    pageNumber = pagedList.CurrentPage + 1,
                    pageSize = pagedList.PageSize,
                    birthPlace = parameters.BirthPlace,
                    searchQuery = parameters.SearchQuery,
                    sortBy = parameters.SortBy
                }) : null
            };
            //将分页的元数据加入到响应的header内,自定义消息头"X-Pagination"
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var authorDtoList = Mapper.Map<IEnumerable<AuthorDto>>(pagedList);
            //为authorDtoList中的每一个资源添加_link属性
            authorDtoList = authorDtoList.Select(authorDto => CreateLinksForAuthor(authorDto));
            var resourceList = new ResourceCollection<AuthorDto>(authorDtoList.ToList());
            return CreateLinksForAuthors(resourceList, parameters, paginationMetadata);
        }

        //[ResponseCache(Duration = 60)] //为请求加了缓存，60秒失效，在响应的header里面会有cache-control max-age = 60
        [ResponseCache(CacheProfileName = "Default")]//"Default"是在StartUp中配置的
        [HttpGet("{authorId}", Name = nameof(GetAuthorAsync))]//在[Route("api/authors")]这个router基础上传入authorId, 即api/authors/xxxxx
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId)
        {
            var author = await RepositoryWrapper.Author.GetByIdAsync(authorId);
            if (author == null)
            {
                return NotFound();
            }

            var entityHash = HashFactory.GetHash(author);
            //响应的header里面etag值就是对象序列化以后的MD5哈希值
            Response.Headers[HeaderNames.ETag] = entityHash;
            //如果请求头中包含IfNoneMatch，并且值与本次计算的ahtor结果的哈希值一样，则返回304，客户端可以用缓存中的内容,否则返回新的author值和200状态码
            if (Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var requestETag) && requestETag == entityHash)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }
            var authorDto = Mapper.Map<AuthorDto>(author);
            //为返回的authorDto添加_link属性
            return CreateLinksForAuthor(authorDto);
        }

        [HttpPost(Name =nameof(CreateAuthorAsync))]
        public async Task<IActionResult> CreateAuthorAsync(AuthorForCreationDto authorForCreationDto)
        {
            var author = Mapper.Map<Author>(authorForCreationDto);
            RepositoryWrapper.Author.Create(author);
            var result = await RepositoryWrapper.Author.SaveAsync();
            if (!result)
            {
                throw new Exception("Create resource author failed!");
            }
            var authorCreated = Mapper.Map<AuthorDto>(author);
            return CreatedAtRoute(nameof(CreateAuthorAsync), new { authorId = authorCreated.Id }, CreateLinksForAuthor(authorCreated));
        }

        [HttpDelete("{authorId}", Name = nameof(DeleteAuthorAsync))]
        public async Task<IActionResult> DeleteAuthorAsync(Guid authorId)
        {
            var author = await RepositoryWrapper.Author.GetByIdAsync(authorId);
            if (author == null)
            {
                return NotFound();
            }
            RepositoryWrapper.Author.Delete(author);
            var result = await RepositoryWrapper.Author.SaveAsync();
            if (!result)
            {
                throw new Exception("Delete resource failed");
            }
            return NoContent();
        }

        [HttpPut("{authorId}")]
        public async Task<IActionResult> UpdateAuthorAsync(Guid authorId, [FromBody] AuthorForUpdateDto updatedAuthor)
        {
            var author = await RepositoryWrapper.Author.GetByIdAsync(authorId);

            if (author == null)
            {
                return NotFound();
            }
            //这个重载能将updatedAuthor map到已经存在的author实体
            Mapper.Map(updatedAuthor, author, typeof(BookForUpdateDto), typeof(Book));
            if (!await RepositoryWrapper.Author.SaveAsync())
            {
                throw new Exception("Update resource author failed!");
            }
            return NoContent();
        }

        private AuthorDto CreateLinksForAuthor(AuthorDto author)
        {
            author.Links.Clear();
            //添加查询author的link
            author.Links.Add(new Link(HttpMethods.Get, "self", Url.Link(nameof(GetAuthorAsync), new { authorId = author.Id })));
            //添加删除author的link
            author.Links.Add(new Link(HttpMethods.Delete, "delete author", Url.Link(nameof(DeleteAuthorAsync), new { authorId = author.Id })));
            //添加查询author下所有book的link
            author.Links.Add(new Link(HttpMethods.Get, "author's book", Url.Link(nameof(BookController.GetBooksAsync), new { authorId = author.Id })));
            return author;
        }

        //给返回的authors集合添加_link属性
        private ResourceCollection<AuthorDto> CreateLinksForAuthors
        (
            ResourceCollection<AuthorDto> authors,
            AuthorResourceParameters parameters = null,
            dynamic paginationData = null
        )
        {
            authors.Links.Clear();
            //如果有查询参数或者过滤参数或者排序参数，则带上这些参数返回到_link
            authors.Links.Add(new Link(HttpMethods.Get, "self", Url.Link(nameof(GetAuthorsAsync), parameters)));
            authors.Links.Add(new Link(HttpMethods.Post, "create author", Url.Link(nameof(CreateAuthorAsync), null)));
            //如果有分页信息，添加分页的_link
            if(paginationData != null)
            {
                if(paginationData.previousePageLink != null)
                {
                    authors.Links.Add(new Link(HttpMethods.Get, "previous page", paginationData.previousePageLink));
                }
                if (paginationData.nextPageLink != null)
                {
                    authors.Links.Add(new Link(HttpMethods.Get, "next page", paginationData.nextPageLink));
                }
            }
            return authors;
        }
    }
}
