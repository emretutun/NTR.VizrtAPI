using Microsoft.AspNetCore.Mvc;
using NTR.Application.DTOs;
using NTR.Core.Enums;
using NTR.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KjController : ControllerBase
    {
        private readonly IVizrtService _vizrtService;

        public KjController(IVizrtService vizrtService)
        {
            _vizrtService = vizrtService;
        }

        [HttpPost("{engineType}/ver")]
        public async Task<IActionResult> Ver(VizrtEngineType engineType, [FromBody] KjRequestDto dto)
        {
            var result = await _vizrtService.SendKjAsync(engineType, dto.Type, dto.Text1, dto.Text2, dto.Rozet);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/al")]
        public async Task<IActionResult> Al(VizrtEngineType engineType)
        {
            var result = await _vizrtService.TakeKjAsync(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/tumunu-al")]
        public async Task<IActionResult> TumunuAl(VizrtEngineType engineType)
        {
            var result = await _vizrtService.TakeAllAsync(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/yer/ver")]
        public async Task<IActionResult> YerVer(VizrtEngineType engineType, [FromBody] YerRequestDto dto)
        {
            var result = await _vizrtService.SendYerAsync(engineType, dto.Text);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/yer/al")]
        public IActionResult YerAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeYer(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/sosyal-medya/ver")]
        public async Task<IActionResult> SosyalMedyaVer(VizrtEngineType engineType)
        {
            var result = await _vizrtService.SendSosyalMedyaAsync(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/sosyal-medya/al")]
        public IActionResult SosyalMedyaAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeSosyalMedya(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/isimlik/ver")]
        public IActionResult IsimlikVer(VizrtEngineType engineType, [FromBody] IsimlikRequestDto dto)
        {
            var result = _vizrtService.SendIsimlik(engineType, dto.Isim);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/isimlik/al")]
        public IActionResult IsimlikAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeIsimlik(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/telefon-isimlik/ver")]
        public async Task<IActionResult> TelefonIsimlikVer(VizrtEngineType engineType, [FromBody] IsimlikRequestDto dto)
        {
            var result = await _vizrtService.SendTelefonIsimlikAsync(engineType, dto.Isim, dto.Title, dto.TelefonMu);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/telefon-isimlik/al")]
        public IActionResult TelefonIsimlikAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeTelefonIsimlik(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/muhabir-kamera/ver")]
        public IActionResult MuhabirKameraVer(VizrtEngineType engineType, [FromBody] MuhabirKameraRequestDto dto)
        {
            var result = _vizrtService.SendMuhabirKamera(engineType, dto.Muhabir, dto.Kameraman);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/muhabir-kamera/al")]
        public IActionResult MuhabirKameraAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeMuhabirKamera(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/canli/ver")]
        public IActionResult CanliVer(VizrtEngineType engineType)
        {
            var result = _vizrtService.SendCanli(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/canli/al")]
        public IActionResult CanliAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeCanli(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/canli-yer/ver")]
        public IActionResult CanliYerVer(VizrtEngineType engineType, [FromBody] YerRequestDto dto)
        {
            var result = _vizrtService.SendCanliYer(engineType, dto.Text);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/canli-yer/al")]
        public IActionResult CanliYerAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeCanliYer(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── ROZETLER ────────────────────────────────────────────

        [HttpPost("{engineType}/rozet/ver")]
        public IActionResult RozetVer(VizrtEngineType engineType, [FromQuery] RozetType rozetType)
        {
            var result = _vizrtService.SendRozet(engineType, rozetType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/rozet/al")]
        public IActionResult RozetAl(VizrtEngineType engineType, [FromQuery] RozetType rozetType)
        {
            var result = _vizrtService.TakeRozet(engineType, rozetType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/rozet/tumunu-al")]
        public IActionResult RozetTumunuAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeAllRozet(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── WHATSAPP ─────────────────────────────────────────────

        [HttpPost("{engineType}/whatsapp/ver")]
        public async Task<IActionResult> WhatsappVer(VizrtEngineType engineType)
        {
            var result = await _vizrtService.SendWhatsappAsync(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/whatsapp/al")]
        public IActionResult WhatsappAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeWhatsapp(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── ROLL ─────────────────────────────────────────────

        [HttpPost("{engineType}/roll/ver")]
        public async Task<IActionResult> RollVer(VizrtEngineType engineType, [FromBody] RollRequestDto dto)
        {
            var satirlar = dto.Satirlar.Select(s => (s.Baslik, s.Yazi)).ToList();
            var result = await _vizrtService.SendRollAsync(engineType, dto.TesekkurYazisi, satirlar, dto.Sponsorlar);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/roll/al")]
        public IActionResult RollAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeRoll(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/roll-tek-metin/ver")]
        public async Task<IActionResult> RollTekMetinVer(VizrtEngineType engineType, [FromBody] RollTekMetinRequestDto dto)
        {
            var result = await _vizrtService.SendRollTekMetinAsync(engineType, dto.RollMetni, dto.Sponsorlar);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── KELEBEK ─────────────────────────────────────────────

        [HttpPost("{engineType}/kelebek/sahne")]
        public IActionResult KelebekSahneYukle(VizrtEngineType engineType, [FromBody] KelebekSahneDto dto)
        {
            var result = _vizrtService.KelebekSahneYukle(engineType, dto.SahneYolu);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/kelebek/isim")]
        public IActionResult KelebekIsimGonder(VizrtEngineType engineType, [FromBody] KelebekIsimDto dto)
        {
            var result = _vizrtService.KelebekIsimGonder(engineType, dto.Index, dto.Isim, dto.Title);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/kelebek/kapat")]
        public IActionResult KelebekKapat(VizrtEngineType engineType)
        {
            var result = _vizrtService.KelebekKapat(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}