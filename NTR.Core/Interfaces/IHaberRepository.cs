using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;

namespace NTR.Core.Interfaces
{
    public interface IHaberRepository
    {
        Task<List<Haber>> GetAllAsync();
        Task<List<Haber>> GetByRundownIdAsync(int rundownId);
        Task<Haber?> GetByIdAsync(int id);
        Task<Haber> AddAsync(Haber haber);
        Task<Haber?> UpdateAsync(Haber haber);
        Task<bool> DeleteAsync(int id);
    }
}