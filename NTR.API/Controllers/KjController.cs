using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NTR.Application.DTOs;
using NTR.Core.Enums;
using NTR.Core.Interfaces;

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
        public IActionResult Ver(VizrtEngineType engineType, [FromBody] KjRequestDto dto)
        {
            var result = _vizrtService.SendKj(engineType, dto.Type, dto.Text1, dto.Text2);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/al")]
        public IActionResult Al(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeKj(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/tumunu-al")]
        public IActionResult TumunuAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeAll(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/yer/ver")]
        public IActionResult YerVer(VizrtEngineType engineType, [FromBody] YerRequestDto dto)
        {
            var result = _vizrtService.SendYer(engineType, dto.Text);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/yer/al")]
        public IActionResult YerAl(VizrtEngineType engineType)
        {
            var result = _vizrtService.TakeYer(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/sosyal-medya/ver")]
        public IActionResult SosyalMedyaVer(VizrtEngineType engineType)
        {
            var result = _vizrtService.SendSosyalMedya(engineType);
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
    }
}