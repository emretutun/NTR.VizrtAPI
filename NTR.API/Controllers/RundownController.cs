using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NTR.Application.Services;
using NTR.Core.Entities;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RundownController : ControllerBase
    {
        private readonly RundownService _rundownService;

        public RundownController(RundownService rundownService)
        {
            _rundownService = rundownService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _rundownService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _rundownService.GetByIdAsync(id);
            if (result == null) return NotFound($"Rundown bulunamadı. Id: {id}");
            return Ok(result);
        }

        [HttpGet("tarih/{tarih}")]
        public async Task<IActionResult> GetByTarih(string tarih)
        {
            var result = await _rundownService.GetByTarihAsync(tarih);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Rundown rundown)
        {
            var result = await _rundownService.AddAsync(rundown);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Rundown rundown)
        {
            var result = await _rundownService.UpdateAsync(rundown);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _rundownService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}