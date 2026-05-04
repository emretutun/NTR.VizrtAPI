using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;
using System.Text.Json;

namespace NTR.Infrastructure.Repositories
{
    public class JsonKjRepository : IKjRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public JsonKjRepository(string filePath)
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions { WriteIndented = true };
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private async Task<List<KjItem>> ReadAsync()
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<KjItem>>(json, _options) ?? new List<KjItem>();
        }

        private async Task WriteAsync(List<KjItem> kjItems)
        {
            string json = JsonSerializer.Serialize(kjItems, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<KjItem>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try { return await ReadAsync(); }
            finally { _lock.Release(); }
        }

        public async Task<List<KjItem>> GetByHaberIdAsync(int haberId)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                return kjItems.Where(k => k.HaberId == haberId).OrderBy(k => k.Sira).ToList();
            }
            finally { _lock.Release(); }
        }

        public async Task<KjItem?> GetByIdAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                return kjItems.FirstOrDefault(k => k.Id == id);
            }
            finally { _lock.Release(); }
        }

        public async Task<KjItem> AddAsync(KjItem kjItem)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                kjItem.Id = kjItems.Count > 0 ? kjItems.Max(k => k.Id) + 1 : 1;
                kjItem.OlusturmaTarihi = DateTime.Now;
                kjItems.Add(kjItem);
                await WriteAsync(kjItems);
                return kjItem;
            }
            finally { _lock.Release(); }
        }

        public async Task<KjItem?> UpdateAsync(KjItem kjItem)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                int index = kjItems.FindIndex(k => k.Id == kjItem.Id);
                if (index == -1) return null;
                kjItems[index] = kjItem;
                await WriteAsync(kjItems);
                return kjItem;
            }
            finally { _lock.Release(); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                int removed = kjItems.RemoveAll(k => k.Id == id);
                if (removed == 0) return false;
                await WriteAsync(kjItems);
                return true;
            }
            finally { _lock.Release(); }
        }

        public async Task<bool> SwapOrderAsync(int id1, int id2)
        {
            await _lock.WaitAsync();
            try
            {
                var kjItems = await ReadAsync();
                var item1 = kjItems.FirstOrDefault(k => k.Id == id1);
                var item2 = kjItems.FirstOrDefault(k => k.Id == id2);
                if (item1 == null || item2 == null) return false;
                (item1.Sira, item2.Sira) = (item2.Sira, item1.Sira);
                await WriteAsync(kjItems);
                return true;
            }
            finally { _lock.Release(); }
        }
    }
}
