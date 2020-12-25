using System;
using BookLib.Services;
using GraphQL.Types;

namespace BookLib.GraphQLSchema
{
    public class LibraryQuery: ObjectGraphType
    {
        public LibraryQuery(IRepositoryWrapper repositoryWrapper)
        {
            Field<ListGraphType<AuthorType>>("authors", resolve: context => repositoryWrapper.Author.GetAllAsync().Result);
            Field<AuthorType>
                (
                    "author",
                    //为author属性创建了一个名为id的参数
                    arguments: new QueryArguments(new QueryArgument<IdGraphType>()
                    {
                        Name = "id"
                    }),
                    resolve: context =>
                    {
                        Guid id = Guid.Empty;
                        //访问context.Arguments获取id参数，context.Arguments是一个Dictionary
                        if (context.Arguments.ContainsKey("id"))
                        {
                            //从context.Arguments中的id参数生成一个Guid类型
                            id = new Guid(context.Arguments["id"].ToString());

                        }
                        //通过从context.Arguments的id属性生成的guid类型，调用repositoryWrapper.Author.GetByIdAsync获得返回值
                        return repositoryWrapper.Author.GetByIdAsync(id).Result;
                    }
                );
        }
    }
}
