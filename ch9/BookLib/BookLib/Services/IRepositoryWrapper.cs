using System;
namespace BookLib.Services
{
    //包装器接口
    public interface IRepositoryWrapper
    {
        IBookRepository Book { get; }
        IAuthorRepository Author { get;}
    }
}
