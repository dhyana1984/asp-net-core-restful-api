using System;
using BookLib.Entities;
using BookLib.Services;
using GraphQL.Types;

namespace BookLib.GraphQLSchema
{
    //ObjectGraphType<Author>表示一个GraphQL对象类型，泛型参数指明了该对象类型所指向的具体实体类型
    public class AuthorType : ObjectGraphType<Author>
    {
        public AuthorType(IRepositoryWrapper repositoryWrapper)
        {
            //利用Field访问实体类的属性，并且为该GraphQL对象向外暴露属性，这些属性将会用于GraphQL查询中
            Field(x => x.Id, type: typeof(IdGraphType));//IdGraphType是表示Id属性
            Field(x => x.Name);
            Field(x => x.BirthDate);
            Field(x => x.BirthPlace);
            Field(x => x.Email);
            //books类型，ListGraphType<BookType>表示GraphQL列表类型
            //resolve:是为"books"属性获取数据，context.Source就是author
            Field<ListGraphType<BookType>>("books", resolve: context => repositoryWrapper.Book.GetBooksAsync(context.Source.Id).Result);
        }
    }
}
