using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Application.Services
{
    public class LogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task InfoAsync(string kaynak, string mesaj, string? detay = null)
        {
            await _logRepository.AddAsync(new LogEntry
            {
                Tarih = DateTime.Now,
                Seviye = "INFO",
                Kaynak = kaynak,
                Mesaj = mesaj,
                Detay = detay
            });
        }

        public async Task WarningAsync(string kaynak, string mesaj, string? detay = null)
        {
            await _logRepository.AddAsync(new LogEntry
            {
                Tarih = DateTime.Now,
                Seviye = "WARNING",
                Kaynak = kaynak,
                Mesaj = mesaj,
                Detay = detay
            });
        }

        public async Task ErrorAsync(string kaynak, string mesaj, string? detay = null)
        {
            await _logRepository.AddAsync(new LogEntry
            {
                Tarih = DateTime.Now,
                Seviye = "ERROR",
                Kaynak = kaynak,
                Mesaj = mesaj,
                Detay = detay
            });
        }

        public async Task<List<LogEntry>> GetTodayAsync()
        {
            return await _logRepository.GetAllAsync();
        }

        public async Task<List<LogEntry>> GetByTarihAsync(DateTime tarih)
        {
            return await _logRepository.GetByTarihAsync(tarih);
        }

        public async Task<List<LogEntry>> GetErrorsAsync()
        {
            return await _logRepository.GetBySeviyeAsync("ERROR");
        }

        public async Task ClearAsync()
        {
            await _logRepository.ClearAsync();
        }

        // Fire and forget - beklemeden log at
        public void Log(string kaynak, string mesaj, string? detay = null)
        {
            _ = InfoAsync(kaynak, mesaj, detay);
        }

        public void Error(string kaynak, string mesaj, string? detay = null)
        {
            _ = ErrorAsync(kaynak, mesaj, detay);
        }

        public void Warning(string kaynak, string mesaj, string? detay = null)
        {
            _ = WarningAsync(kaynak, mesaj, detay);
        }
    }
}