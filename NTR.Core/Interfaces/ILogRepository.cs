using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NTR.Core.Entities;

namespace NTR.Core.Interfaces
{
    public interface ILogRepository
    {
        Task AddAsync(LogEntry entry);
        Task<List<LogEntry>> GetAllAsync();
        Task<List<LogEntry>> GetBySeviyeAsync(string seviye);
        Task<List<LogEntry>> GetByTarihAsync(DateTime tarih);
        Task ClearAsync();
    }
}