using System;
using System.Collections.Generic;
using System.Globalization;
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

        // Hem read hem write aynı anda kontrollü olsun
        private readonly SemaphoreSlim _semaphore = new(1, 1);

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
               (string.IsNullOrWhiteSpace(entry.Detay)
                   ? ""
                   : $" | {entry.Detay}");

        public async Task AddAsync(LogEntry entry)
        {
            await _semaphore.WaitAsync();

            try
            {
                string line = FormatLog(entry) + Environment.NewLine;

                // Günlük log
                await AppendSafeAsync(GetDailyLogPath(), line);

                // Error log
                if (entry.Seviye.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    await AppendSafeAsync(GetErrorLogPath(), line);
                }
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

            return all
                .Where(x => x.Seviye.Equals(seviye, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
                {
                    File.Delete(path);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task AppendSafeAsync(string path, string content)
        {
            using var stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite,
                4096,
                useAsync: true);

            using var writer = new StreamWriter(stream);

            await writer.WriteAsync(content);
            await writer.FlushAsync();
        }

        private async Task<List<LogEntry>> ReadFromFile(string path)
        {
            var entries = new List<LogEntry>();

            if (!File.Exists(path))
                return entries;

            await _semaphore.WaitAsync();

            try
            {
                List<string> lines;

                using (var stream = new FileStream(
                           path,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.ReadWrite,
                           4096,
                           useAsync: true))
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();

                    lines = content
                        .Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                int id = 1;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        // Format:
                        // [2026-05-08 12:00:00] [INFO   ] [Engine] mesaj | detay

                        string tarihStr = line.Substring(1, 19);

                        string seviye = line
                            .Substring(22, 7)
                            .Replace("[", "")
                            .Replace("]", "")
                            .Trim();

                        int kaynakStart = line.IndexOf("] [", 21) + 3;
                        int kaynakEnd = line.IndexOf("]", kaynakStart);

                        string kaynak = line.Substring(
                            kaynakStart,
                            kaynakEnd - kaynakStart);

                        string rest = line
                            .Substring(kaynakEnd + 2)
                            .Trim();

                        string mesaj = rest.Contains(" | ")
                            ? rest.Split(" | ")[0]
                            : rest;

                        string? detay = rest.Contains(" | ")
                            ? rest.Split(" | ")[1]
                            : null;

                        entries.Add(new LogEntry
                        {
                            Id = id++,
                            Tarih = DateTime.ParseExact(
                                tarihStr,
                                "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture),

                            Seviye = seviye,
                            Kaynak = kaynak,
                            Mesaj = mesaj,
                            Detay = detay
                        });
                    }
                    catch
                    {
                        // parse edilemeyen satırı geç
                    }
                }

                return entries;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}