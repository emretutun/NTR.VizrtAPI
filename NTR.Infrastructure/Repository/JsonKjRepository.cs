using System.Text.Json;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Infrastructure.Repositories
{
    public class JsonKjRepository : IKjRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public JsonKjRepository(string filePath)
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

        private async Task<List<KjItem>> ReadAsync()
        {
            using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            using var reader = new StreamReader(stream);

            string json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<KjItem>>(json, _options)
                   ?? new List<KjItem>();
        }

        private async Task WriteAsync(List<KjItem> kjItems)
        {
            string json = JsonSerializer.Serialize(kjItems, _options);

            string tempPath = _filePath + ".tmp";

            await File.WriteAllTextAsync(tempPath, json);

            File.Copy(tempPath, _filePath, true);

            File.Delete(tempPath);
        }

        public async Task<List<KjItem>> GetAllAsync()
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

        public async Task<List<KjItem>> GetByHaberIdAsync(int haberId)
        {
            await _lock.WaitAsync();

            try
            {
                var kjItems = await ReadAsync();

                return kjItems
                    .Where(k => k.HaberId == haberId)
                    .OrderBy(k => k.Sira)
                    .ToList();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<KjItem?> GetByIdAsync(int id)
        {
            await _lock.WaitAsync();

            try
            {
                var kjItems = await ReadAsync();

                return kjItems.FirstOrDefault(k => k.Id == id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<KjItem> AddAsync(KjItem kjItem)
        {
            await _lock.WaitAsync();

            try
            {
                var kjItems = await ReadAsync();

                kjItem.Id = kjItems.Count > 0
                    ? kjItems.Max(k => k.Id) + 1
                    : 1;

                kjItem.OlusturmaTarihi = DateTime.Now;

                kjItems.Add(kjItem);

                await WriteAsync(kjItems);

                return kjItem;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<KjItem?> UpdateAsync(KjItem kjItem)
        {
            await _lock.WaitAsync();

            try
            {
                var kjItems = await ReadAsync();

                int index = kjItems.FindIndex(k => k.Id == kjItem.Id);

                if (index == -1)
                    return null;

                kjItems[index] = kjItem;

                await WriteAsync(kjItems);

                return kjItem;
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
                var kjItems = await ReadAsync();

                int removed = kjItems.RemoveAll(k => k.Id == id);

                if (removed == 0)
                    return false;

                await WriteAsync(kjItems);

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> SwapOrderAsync(int id1, int id2)
        {
            await _lock.WaitAsync();

            try
            {
                var kjItems = await ReadAsync();

                var item1 = kjItems.FirstOrDefault(k => k.Id == id1);
                var item2 = kjItems.FirstOrDefault(k => k.Id == id2);

                if (item1 == null || item2 == null)
                    return false;

                (item1.Sira, item2.Sira) =
                    (item2.Sira, item1.Sira);

                await WriteAsync(kjItems);

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}