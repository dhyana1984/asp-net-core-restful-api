using System;
using BookLib.Entities;
using GraphQL.Types;

namespace BookLib.GraphQLSchema
{
    public class BookType : ObjectGraphType<Book>
    {
        public BookType()
        {
            Field(x => x.Id, type: typeof(IdGraphType));
            Field(x => x.Title);
            Field(x => x.Description);
            Field(x => x.Pages);
        }
    }
}
