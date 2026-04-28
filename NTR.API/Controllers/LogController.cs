using Microsoft.AspNetCore.Mvc;
using NTR.Application.Services;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly LogService _logService;

        public LogController(LogService logService)
        {
            _logService = logService;
        }

        [HttpGet("bugun")]
        public async Task<IActionResult> GetBugun()
        {
            var result = await _logService.GetTodayAsync();
            return Ok(result);
        }

        [HttpGet("tarih/{tarih}")]
        public async Task<IActionResult> GetByTarih(string tarih)
        {
            if (!DateTime.TryParse(tarih, out DateTime dt))
                return BadRequest("Geçersiz tarih formatı. Örnek: 2026-04-28");

            var result = await _logService.GetByTarihAsync(dt);
            return Ok(result);
        }

        [HttpGet("hatalar")]
        public async Task<IActionResult> GetHatalar()
        {
            var result = await _logService.GetErrorsAsync();
            return Ok(result);
        }

        [HttpDelete("temizle")]
        public async Task<IActionResult> Temizle()
        {
            await _logService.ClearAsync();
            return Ok("Günlük loglar temizlendi.");
        }
    }
}