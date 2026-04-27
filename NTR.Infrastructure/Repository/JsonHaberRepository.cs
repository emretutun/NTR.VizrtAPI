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
    public class JsonHaberRepository : IHaberRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonHaberRepository(string filePath)
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

        private async Task<List<Haber>> ReadAsync()
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Haber>>(json, _options) ?? new List<Haber>();
        }

        private async Task WriteAsync(List<Haber> haberler)
        {
            string json = JsonSerializer.Serialize(haberler, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<Haber>> GetAllAsync()
        {
            return await ReadAsync();
        }

        public async Task<List<Haber>> GetByRundownIdAsync(int rundownId)
        {
            var haberler = await ReadAsync();
            return haberler.Where(h => h.RundownId == rundownId).OrderBy(h => h.Sira).ToList();
        }

        public async Task<Haber?> GetByIdAsync(int id)
        {
            var haberler = await ReadAsync();
            return haberler.FirstOrDefault(h => h.Id == id);
        }

        public async Task<Haber> AddAsync(Haber haber)
        {
            var haberler = await ReadAsync();
            haber.Id = haberler.Count > 0 ? haberler.Max(h => h.Id) + 1 : 1;
            haber.OlusturmaTarihi = DateTime.Now;
            haberler.Add(haber);
            await WriteAsync(haberler);
            return haber;
        }

        public async Task<Haber?> UpdateAsync(Haber haber)
        {
            var haberler = await ReadAsync();
            int index = haberler.FindIndex(h => h.Id == haber.Id);
            if (index == -1) return null;
            haberler[index] = haber;
            await WriteAsync(haberler);
            return haber;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var haberler = await ReadAsync();
            int removed = haberler.RemoveAll(h => h.Id == id);
            if (removed == 0) return false;
            await WriteAsync(haberler);
            return true;
        }
    }
}