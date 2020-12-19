using System;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace BookLib.GraphQLSchema
{
    public class LibrarySchema : Schema
    {
        //在.net core中使用IServiceProvider 并且传给base
        public LibrarySchema(LibraryQuery query, IServiceProvider provider) : base(provider)
        {
            Query = query;
        }

        //这是旧版.net mvc 中的写法， IDependencyResolver 在.net core中没有
        //public LibrarySchema(LibraryQuery query, IDependencyResolver dependencyResolver)
        //{
        //    Query = query;
        //    DependencyResolver = dependencyResolver;
        //}
    }
}
