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
    public class JsonRundownRepository : IRundownRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonRundownRepository(string filePath)
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

        private async Task<List<Rundown>> ReadAsync()
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Rundown>>(json, _options) ?? new List<Rundown>();
        }

        private async Task WriteAsync(List<Rundown> rundowns)
        {
            string json = JsonSerializer.Serialize(rundowns, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<List<Rundown>> GetAllAsync()
        {
            return await ReadAsync();
        }

        public async Task<List<Rundown>> GetByTarihAsync(string tarih)
        {
            var rundowns = await ReadAsync();
            return rundowns.Where(r => r.Tarih == tarih).ToList();
        }

        public async Task<Rundown?> GetByIdAsync(int id)
        {
            var rundowns = await ReadAsync();
            return rundowns.FirstOrDefault(r => r.Id == id);
        }

        public async Task<Rundown> AddAsync(Rundown rundown)
        {
            var rundowns = await ReadAsync();
            rundown.Id = rundowns.Count > 0 ? rundowns.Max(r => r.Id) + 1 : 1;
            rundown.OlusturmaTarihi = DateTime.Now;
            rundowns.Add(rundown);
            await WriteAsync(rundowns);
            return rundown;
        }

        public async Task<Rundown?> UpdateAsync(Rundown rundown)
        {
            var rundowns = await ReadAsync();
            int index = rundowns.FindIndex(r => r.Id == rundown.Id);
            if (index == -1) return null;
            rundowns[index] = rundown;
            await WriteAsync(rundowns);
            return rundown;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rundowns = await ReadAsync();
            int removed = rundowns.RemoveAll(r => r.Id == id);
            if (removed == 0) return false;
            await WriteAsync(rundowns);
            return true;
        }
    }
}