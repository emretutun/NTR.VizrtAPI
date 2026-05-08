using System.Text.Json;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Infrastructure.Repositories
{
    public class JsonHaberRepository : IHaberRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public JsonHaberRepository(string filePath)
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

        private async Task<List<Haber>> ReadAsync()
        {
            using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            using var reader = new StreamReader(stream);

            string json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<Haber>>(json, _options)
                   ?? new List<Haber>();
        }

        private async Task WriteAsync(List<Haber> haberler)
        {
            string json = JsonSerializer.Serialize(haberler, _options);

            string tempPath = _filePath + ".tmp";

            await File.WriteAllTextAsync(tempPath, json);

            File.Copy(tempPath, _filePath, true);

            File.Delete(tempPath);
        }

        public async Task<List<Haber>> GetAllAsync()
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

        public async Task<List<Haber>> GetByRundownIdAsync(int rundownId)
        {
            await _lock.WaitAsync();

            try
            {
                var haberler = await ReadAsync();

                return haberler
                    .Where(h => h.RundownId == rundownId)
                    .OrderBy(h => h.Sira)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Haber?> GetByIdAsync(int id)
        {
            await _lock.WaitAsync();

            try
            {
                var haberler = await ReadAsync();

                return haberler.FirstOrDefault(h => h.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Haber> AddAsync(Haber haber)
        {
            await _lock.WaitAsync();

            try
            {
                var haberler = await ReadAsync();

                haber.Id = haberler.Count > 0
                    ? haberler.Max(h => h.Id) + 1
                    : 1;

                haber.OlusturmaTarihi = DateTime.Now;

                haberler.Add(haber);

                await WriteAsync(haberler);

                return haber;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Haber?> UpdateAsync(Haber haber)
        {
            await _lock.WaitAsync();

            try
            {
                var haberler = await ReadAsync();

                int index = haberler.FindIndex(h => h.Id == haber.Id);

                if (index == -1)
                    return null;

                haberler[index] = haber;

                await WriteAsync(haberler);

                return haber;
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
                var haberler = await ReadAsync();

                int removed = haberler.RemoveAll(h => h.Id == id);

                if (removed == 0)
                    return false;

                await WriteAsync(haberler);

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}