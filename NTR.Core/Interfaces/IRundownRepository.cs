using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;

namespace NTR.Core.Interfaces
{
    public interface IRundownRepository
    {
        Task<List<Rundown>> GetAllAsync();
        Task<List<Rundown>> GetByTarihAsync(string tarih);
        Task<Rundown?> GetByIdAsync(int id);
        Task<Rundown> AddAsync(Rundown rundown);
        Task<Rundown?> UpdateAsync(Rundown rundown);
        Task<bool> DeleteAsync(int id);
    }
}