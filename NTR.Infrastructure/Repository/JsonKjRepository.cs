using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return await ReadAsync();
        }

        public async Task<List<KjItem>> GetByHaberIdAsync(int haberId)
        {
            var kjItems = await ReadAsync();
            return kjItems.Where(k => k.HaberId == haberId).OrderBy(k => k.Sira).ToList();
        }

        public async Task<KjItem?> GetByIdAsync(int id)
        {
            var kjItems = await ReadAsync();
            return kjItems.FirstOrDefault(k => k.Id == id);
        }

        public async Task<KjItem> AddAsync(KjItem kjItem)
        {
            var kjItems = await ReadAsync();
            kjItem.Id = kjItems.Count > 0 ? kjItems.Max(k => k.Id) + 1 : 1;
            kjItem.OlusturmaTarihi = DateTime.Now;
            kjItems.Add(kjItem);
            await WriteAsync(kjItems);
            return kjItem;
        }

        public async Task<KjItem?> UpdateAsync(KjItem kjItem)
        {
            var kjItems = await ReadAsync();
            int index = kjItems.FindIndex(k => k.Id == kjItem.Id);
            if (index == -1) return null;
            kjItems[index] = kjItem;
            await WriteAsync(kjItems);
            return kjItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var kjItems = await ReadAsync();
            int removed = kjItems.RemoveAll(k => k.Id == id);
            if (removed == 0) return false;
            await WriteAsync(kjItems);
            return true;
        }

        public async Task<bool> SwapOrderAsync(int id1, int id2)
        {
            var kjItems = await ReadAsync();
            var item1 = kjItems.FirstOrDefault(k => k.Id == id1);
            var item2 = kjItems.FirstOrDefault(k => k.Id == id2);
            if (item1 == null || item2 == null) return false;

            (item1.Sira, item2.Sira) = (item2.Sira, item1.Sira);
            await WriteAsync(kjItems);
            return true;
        }
    }
}