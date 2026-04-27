using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;

namespace NTR.Core.Interfaces
{
    public interface IKjRepository
    {
        Task<List<KjItem>> GetAllAsync();
        Task<List<KjItem>> GetByHaberIdAsync(int haberId);
        Task<KjItem?> GetByIdAsync(int id);
        Task<KjItem> AddAsync(KjItem kjItem);
        Task<KjItem?> UpdateAsync(KjItem kjItem);
        Task<bool> DeleteAsync(int id);
        Task<bool> SwapOrderAsync(int id1, int id2);
    }
}