using System;
using System.Collections.Generic;
using BookLib.Entities;
using BookLib.Models;

namespace BookLib.Services
{
    public interface IBookRepository: IRepositoryBase<Book>, IRepositoryBase2<Book, Guid>
    {

    }
}
