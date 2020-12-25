using System;
using BookLib.GraphQLSchema;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;


namespace BookLib.Extensions
{
    public static class GraphQLExtension
    {
        public static void AddGraphQLSchemaAndTypes(this IServiceCollection services)
        {
            services.AddTransient<AuthorType>();
            services.AddTransient<BookType>();
            services.AddTransient<LibraryQuery>();
            services.AddTransient<ISchema, LibrarySchema>();
            services.AddTransient<IDocumentExecuter, DocumentExecuter>();
        }
    }
}
