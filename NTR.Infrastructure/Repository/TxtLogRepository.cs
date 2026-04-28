using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Infrastructure.Repositories
{
    public class TxtLogRepository : ILogRepository
    {
        private readonly string _logFolder;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TxtLogRepository(string logFolder)
        {
            _logFolder = logFolder;
            Directory.CreateDirectory(_logFolder);
        }

        private string GetDailyLogPath()
            => Path.Combine(_logFolder, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

        private string GetErrorLogPath()
            => Path.Combine(_logFolder, "log_errors.txt");

        private string FormatLog(LogEntry entry)
            => $"[{entry.Tarih:yyyy-MM-dd HH:mm:ss}] [{entry.Seviye,-7}] [{entry.Kaynak}] {entry.Mesaj}" +
               (string.IsNullOrEmpty(entry.Detay) ? "" : $" | {entry.Detay}");

        public async Task AddAsync(LogEntry entry)
        {
            await _semaphore.WaitAsync();
            try
            {
                string line = FormatLog(entry) + Environment.NewLine;

                // Günlük dosyaya yaz
                await File.AppendAllTextAsync(GetDailyLogPath(), line);

                // Hata ise ayrıca error dosyasına da yaz
                if (entry.Seviye == "ERROR")
                    await File.AppendAllTextAsync(GetErrorLogPath(), line);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LogEntry>> GetAllAsync()
        {
            return await ReadFromFile(GetDailyLogPath());
        }

        public async Task<List<LogEntry>> GetBySeviyeAsync(string seviye)
        {
            var all = await GetAllAsync();
            return all.Where(l => l.Seviye == seviye.ToUpper()).ToList();
        }

        public async Task<List<LogEntry>> GetByTarihAsync(DateTime tarih)
        {
            string path = Path.Combine(_logFolder, $"log_{tarih:yyyy-MM-dd}.txt");
            return await ReadFromFile(path);
        }

        public async Task ClearAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                string path = GetDailyLogPath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<LogEntry>> ReadFromFile(string path)
        {
            var entries = new List<LogEntry>();
            if (!File.Exists(path)) return entries;

            var lines = await File.ReadAllLinesAsync(path);
            int id = 1;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    // [2026-04-28 14:35:22] [INFO   ] [Engine] mesaj | detay
                    string tarihStr = line.Substring(1, 19);
                    string seviye = line.Substring(22, 7).Trim().Replace("]", "").Replace("[", "");
                    int kaynakStart = line.IndexOf("] [", 21) + 3;
                    int kaynakEnd = line.IndexOf("]", kaynakStart);
                    string kaynak = line.Substring(kaynakStart, kaynakEnd - kaynakStart);
                    string rest = line.Substring(kaynakEnd + 2).Trim();
                    string mesaj = rest.Contains(" | ") ? rest.Split(" | ")[0] : rest;
                    string? detay = rest.Contains(" | ") ? rest.Split(" | ")[1] : null;

                    entries.Add(new LogEntry
                    {
                        Id = id++,
                        Tarih = DateTime.Parse(tarihStr),
                        Seviye = seviye,
                        Kaynak = kaynak,
                        Mesaj = mesaj,
                        Detay = detay
                    });
                }
                catch { }
            }
            return entries;
        }
    }
}