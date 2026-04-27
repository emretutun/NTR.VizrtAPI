using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Application.Services
{
    public class HaberService
    {
        private readonly IHaberRepository _haberRepository;
        private readonly IKjRepository _kjRepository;

        public HaberService(IHaberRepository haberRepository, IKjRepository kjRepository)
        {
            _haberRepository = haberRepository;
            _kjRepository = kjRepository;
        }

        public async Task<List<Haber>> GetAllAsync()
        {
            return await _haberRepository.GetAllAsync();
        }

        public async Task<List<Haber>> GetByRundownIdAsync(int rundownId)
        {
            var haberler = await _haberRepository.GetByRundownIdAsync(rundownId);
            foreach (var haber in haberler)
                haber.KjListesi = await _kjRepository.GetByHaberIdAsync(haber.Id);
            return haberler;
        }

        public async Task<Haber?> GetByIdAsync(int id)
        {
            var haber = await _haberRepository.GetByIdAsync(id);
            if (haber == null) return null;
            haber.KjListesi = await _kjRepository.GetByHaberIdAsync(haber.Id);
            return haber;
        }

        public async Task<CommandResult> AddAsync(Haber haber)
        {
            try
            {
                var added = await _haberRepository.AddAsync(haber);
                return CommandResult.Ok("Haber eklendi.", added);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Haber eklenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> UpdateAsync(Haber haber)
        {
            try
            {
                var existing = await _haberRepository.GetByIdAsync(haber.Id);
                if (existing == null)
                    return CommandResult.Fail($"Haber bulunamadı. Id: {haber.Id}");

                var updated = await _haberRepository.UpdateAsync(haber);
                return CommandResult.Ok("Haber güncellendi.", updated);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Haber güncellenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> DeleteAsync(int id)
        {
            try
            {
                var existing = await _haberRepository.GetByIdAsync(id);
                if (existing == null)
                    return CommandResult.Fail($"Haber bulunamadı. Id: {id}");

                // habere ait KJ'leri de sil
                var kjler = await _kjRepository.GetByHaberIdAsync(id);
                foreach (var kj in kjler)
                    await _kjRepository.DeleteAsync(kj.Id);

                await _haberRepository.DeleteAsync(id);
                return CommandResult.Ok("Haber ve bağlı KJ'ler silindi.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Haber silinemedi: {ex.Message}");
            }
        }
    }
}