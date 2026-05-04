using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        public SeatsController(ISeatService seatService) => _seatService = seatService;

        [HttpGet("{busId}")]
        public async Task<IActionResult> GetSeatMap(Guid busId) => Ok(await _seatService.GetSeatMapAsync(busId));

        [HttpPost("lock")]
        [Authorize]
        public async Task<IActionResult> LockSeats([FromBody] LockSeatRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _seatService.LockSeatsAsync(request.BusId, request.SeatNumbers, userId);
            return result.Success ? Ok(result) : Conflict(result);
        }

        [HttpDelete("lock/{lockId}")]
        [Authorize]
        public async Task<IActionResult> ReleaseLock(Guid lockId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _seatService.ReleaseLockAsync(lockId, userId));
        }
    }
}
