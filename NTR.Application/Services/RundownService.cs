using NTR.Core.Entities;
using NTR.Core.Interfaces;

namespace NTR.Application.Services
{
    public class RundownService
    {
        private readonly IRundownRepository _rundownRepository;
        private readonly IHaberRepository _haberRepository;
        private readonly IKjRepository _kjRepository;

        public RundownService(
            IRundownRepository rundownRepository,
            IHaberRepository haberRepository,
            IKjRepository kjRepository)
        {
            _rundownRepository = rundownRepository;
            _haberRepository = haberRepository;
            _kjRepository = kjRepository;
        }

        public async Task<List<Rundown>> GetAllAsync()
        {
            return await _rundownRepository.GetAllAsync();
        }

        public async Task<List<Rundown>> GetByTarihAsync(string tarih)
        {
            return await _rundownRepository.GetByTarihAsync(tarih);
        }

        public async Task<Rundown?> GetByIdAsync(int id)
        {
            var rundown = await _rundownRepository.GetByIdAsync(id);
            if (rundown == null) return null;

            var haberler = await _haberRepository.GetByRundownIdAsync(id);
            foreach (var haber in haberler)
                haber.KjListesi = await _kjRepository.GetByHaberIdAsync(haber.Id);

            rundown.Haberler = haberler;
            return rundown;
        }

        public async Task<CommandResult> AddAsync(Rundown rundown)
        {
            try
            {
                var added = await _rundownRepository.AddAsync(rundown);
                return CommandResult.Ok("Rundown eklendi.", added);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Rundown eklenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> UpdateAsync(Rundown rundown)
        {
            try
            {
                var existing = await _rundownRepository.GetByIdAsync(rundown.Id);
                if (existing == null)
                    return CommandResult.Fail($"Rundown bulunamadı. Id: {rundown.Id}");

                var updated = await _rundownRepository.UpdateAsync(rundown);
                return CommandResult.Ok("Rundown güncellendi.", updated);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Rundown güncellenemedi: {ex.Message}");
            }
        }

        public async Task<CommandResult> DeleteAsync(int id)
        {
            try
            {
                var existing = await _rundownRepository.GetByIdAsync(id);
                if (existing == null)
                    return CommandResult.Fail($"Rundown bulunamadı. Id: {id}");

                // rundown'a ait haberleri ve KJ'leri de sil
                var haberler = await _haberRepository.GetByRundownIdAsync(id);
                foreach (var haber in haberler)
                {
                    var kjler = await _kjRepository.GetByHaberIdAsync(haber.Id);
                    foreach (var kj in kjler)
                        await _kjRepository.DeleteAsync(kj.Id);
                    await _haberRepository.DeleteAsync(haber.Id);
                }

                await _rundownRepository.DeleteAsync(id);
                return CommandResult.Ok("Rundown ve bağlı tüm veriler silindi.");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Rundown silinemedi: {ex.Message}");
            }
        }
    }
}