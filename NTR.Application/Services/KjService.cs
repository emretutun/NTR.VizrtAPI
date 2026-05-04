using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Application.Services
{
    public class KjService
    {
        private readonly IKjRepository _kjRepository;
        private readonly IHaberRepository _haberRepository;

        public KjService(IKjRepository kjRepository, IHaberRepository haberRepository)
        {
            _kjRepository = kjRepository;
            _haberRepository = haberRepository;
        }

        public async Task<List<KjItem>> GetByHaberIdAsync(int haberId)
        {
            return await _kjRepository.GetByHaberIdAsync(haberId);
        }

        public async Task<KjItem?> GetByIdAsync(int id)
        {
            return await _kjRepository.GetByIdAsync(id);
        }

        public async Task<CommandResult> AddAsync(KjItem kjItem)
        {
            try
            {
                var haber = await _haberRepository.GetByIdAsync(kjItem.HaberId);
                if (haber == null)
                    return CommandResult.Fail($"Haber bulunamadı. HaberId: {kjItem.HaberId}");

                var kjler = await _kjRepository.GetByHaberIdAsync(kjItem.HaberId);
                kjItem.Sira = kjler.Count > 0 ? kjler.Max(k => k.Sira) + 1 : 1;

                var added = await _kjRepository.AddAsync(kjItem);
                return CommandResult.Ok("KJ eklendi.", added);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"KJ eklenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> UpdateAsync(KjItem kjItem)
        {
            try
            {
                // Önce varlık kontrolü yapıyoruz ama dosyayı LOCK altında tek seferde güncelliyoruz
                var updated = await _kjRepository.UpdateAsync(kjItem);
                if (updated == null)
                    return CommandResult.Fail($"KJ bulunamadı. Id: {kjItem.Id}");

                return CommandResult.Ok("KJ güncellendi.", updated);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"KJ güncellenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> DeleteAsync(int id)
        {
            try
            {
                // GetByIdAsync + DeleteAsync yerine doğrudan DeleteAsync çağırıyoruz
                // Repository zaten LOCK altında atomik olarak kontrol edip siliyor
                bool deleted = await _kjRepository.DeleteAsync(id);
                if (!deleted)
                    return CommandResult.Fail($"KJ bulunamadı. Id: {id}");

                return CommandResult.Ok("KJ silindi.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"KJ silinemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> SwapOrderAsync(int id1, int id2)
        {
            try
            {
                var item1 = await _kjRepository.GetByIdAsync(id1);
                var item2 = await _kjRepository.GetByIdAsync(id2);

                if (item1 == null || item2 == null)
                    return CommandResult.Fail("KJ bulunamadı.");

                if (item1.HaberId != item2.HaberId)
                    return CommandResult.Fail("KJ'ler aynı habere ait değil.");

                await _kjRepository.SwapOrderAsync(id1, id2);
                return CommandResult.Ok("KJ sırası değiştirildi.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Sıra değiştirilemedi: {ex.Message}");
            }
        }
    }
}
