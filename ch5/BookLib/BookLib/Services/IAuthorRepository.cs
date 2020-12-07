using System;
using System.Collections.Generic;
using BookLib.Entities;
using BookLib.Models;

namespace BookLib.Services
{
    public interface IAuthorRepository:IRepositoryBase<Author>, IRepositoryBase2<Author,Guid>
    {

    }
}
