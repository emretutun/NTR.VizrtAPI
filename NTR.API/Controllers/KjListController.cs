using Microsoft.AspNetCore.Mvc;
using NTR.Application.Services;
using NTR.Core.Entities;

namespace NTR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KjListController : ControllerBase
    {
        private readonly KjService _kjService;

        public KjListController(KjService kjService)
        {
            _kjService = kjService;
        }

        [HttpGet("haber/{haberId}")]
        public async Task<IActionResult> GetByHaberId(int haberId)
        {
            var result = await _kjService.GetByHaberIdAsync(haberId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _kjService.GetByIdAsync(id);
            if (result == null) return NotFound($"KJ bulunamadı. Id: {id}");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] KjItem kjItem)
        {
            var result = await _kjService.AddAsync(kjItem);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] KjItem kjItem)
        {
            var result = await _kjService.UpdateAsync(kjItem);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _kjService.DeleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("swap")]
        public async Task<IActionResult> Swap([FromQuery] int id1, [FromQuery] int id2)
        {
            var result = await _kjService.SwapOrderAsync(id1, id2);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}