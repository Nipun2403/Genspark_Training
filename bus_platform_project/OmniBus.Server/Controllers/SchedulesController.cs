using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;
using System.Security.Claims;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Operator")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleRequest request)
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _scheduleService.CreateScheduleAsync(request, opId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _scheduleService.GetMySchedulesAsync(opId));
        }

        [HttpPatch("{scheduleId}/toggle")]
        public async Task<IActionResult> Toggle(Guid scheduleId, [FromQuery] bool isActive)
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _scheduleService.ToggleScheduleAsync(scheduleId, isActive, opId));
        }

        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> Delete(Guid scheduleId)
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _scheduleService.DeleteScheduleAsync(scheduleId, opId));
        }
    }
}
