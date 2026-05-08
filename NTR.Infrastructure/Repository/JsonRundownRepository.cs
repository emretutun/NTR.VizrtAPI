using System.Text.Json;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Infrastructure.Repositories
{
    public class JsonRundownRepository : IRundownRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public JsonRundownRepository(string filePath)
        {
            _filePath = filePath;

            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            string? dir = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private async Task<List<Rundown>> ReadAsync()
        {
            using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            using var reader = new StreamReader(stream);

            string json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<Rundown>>(json, _options)
                   ?? new List<Rundown>();
        }

        private async Task WriteAsync(List<Rundown> rundowns)
        {
            string json = JsonSerializer.Serialize(rundowns, _options);

            string tempPath = _filePath + ".tmp";

            await File.WriteAllTextAsync(tempPath, json);

            File.Copy(tempPath, _filePath, true);

            File.Delete(tempPath);
        }

        public async Task<List<Rundown>> GetAllAsync()
        {
            await _lock.WaitAsync();

            try
            {
                return await ReadAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<Rundown>> GetByTarihAsync(string tarih)
        {
            await _lock.WaitAsync();

            try
            {
                var rundowns = await ReadAsync();

                return rundowns
                    .Where(r => r.Tarih == tarih)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Rundown?> GetByIdAsync(int id)
        {
            await _lock.WaitAsync();

            try
            {
                var rundowns = await ReadAsync();

                return rundowns.FirstOrDefault(r => r.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Rundown> AddAsync(Rundown rundown)
        {
            await _lock.WaitAsync();

            try
            {
                var rundowns = await ReadAsync();

                rundown.Id = rundowns.Count > 0
                    ? rundowns.Max(r => r.Id) + 1
                    : 1;

                rundown.OlusturmaTarihi = DateTime.Now;

                rundowns.Add(rundown);

                await WriteAsync(rundowns);

                return rundown;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Rundown?> UpdateAsync(Rundown rundown)
        {
            await _lock.WaitAsync();

            try
            {
                var rundowns = await ReadAsync();

                int index = rundowns.FindIndex(r => r.Id == rundown.Id);

                if (index == -1)
                    return null;

                rundowns[index] = rundown;

                await WriteAsync(rundowns);

                return rundown;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _lock.WaitAsync();

            try
            {
                var rundowns = await ReadAsync();

                int removed = rundowns.RemoveAll(r => r.Id == id);

                if (removed == 0)
                    return false;

                await WriteAsync(rundowns);

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}