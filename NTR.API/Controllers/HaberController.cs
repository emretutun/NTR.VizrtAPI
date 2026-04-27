using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NTR.Application.Services;
using NTR.Core.Entities;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HaberController : ControllerBase
    {
        private readonly HaberService _haberService;

        public HaberController(HaberService haberService)
        {
            _haberService = haberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _haberService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _haberService.GetByIdAsync(id);
            if (result == null) return NotFound($"Haber bulunamadı. Id: {id}");
            return Ok(result);
        }

        [HttpGet("rundown/{rundownId}")]
        public async Task<IActionResult> GetByRundownId(int rundownId)
        {
            var result = await _haberService.GetByRundownIdAsync(rundownId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Haber haber)
        {
            var result = await _haberService.AddAsync(haber);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Haber haber)
        {
            var result = await _haberService.UpdateAsync(haber);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _haberService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}