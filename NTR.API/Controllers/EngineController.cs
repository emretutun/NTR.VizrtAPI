using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NTR.Application.DTOs;
using NTR.Core.Enums;
using NTR.Core.Interfaces;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EngineController : ControllerBase
    {
        private readonly IVizrtService _vizrtService;

        public EngineController(IVizrtService vizrtService)
        {
            _vizrtService = vizrtService;
        }

        [HttpGet("status")]
        public IActionResult GetAllStatus()
        {
            var result = _vizrtService.GetAllEngineStatus();
            return Ok(result);
        }

        [HttpGet("status/{engineType}")]
        public IActionResult GetStatus(VizrtEngineType engineType)
        {
            var result = _vizrtService.GetEngineStatus(engineType);
            return Ok(result);
        }

        [HttpPost("{engineType}/connect")]
        public IActionResult Connect(VizrtEngineType engineType, [FromBody] ConnectRequestDto dto)
        {
            string ip = dto.Port != 6100
                ? $"{dto.IP}:{dto.Port}"
                : dto.IP;

            var result = _vizrtService.Connect(engineType, ip);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/disconnect")]
        public IActionResult Disconnect(VizrtEngineType engineType)
        {
            var result = _vizrtService.Disconnect(engineType);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/raw")]
        public IActionResult SendRaw(VizrtEngineType engineType, [FromBody] RawCommandDto dto)
        {
            var result = _vizrtService.SendRawCommand(engineType, dto.Command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{engineType}/scene/load")]
        public IActionResult LoadScene(VizrtEngineType engineType, [FromBody] string scenePath)
        {
            var result = _vizrtService.LoadScene(engineType, scenePath);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}