using System;
using System.Threading.Tasks;

namespace BookLib.Services
{
    public interface IRepositoryBase2<T,TId>
    {
        Task<T> GetByIdAsync(TId id);
        Task<bool> IsExistAsync(TId id);
    }
}
