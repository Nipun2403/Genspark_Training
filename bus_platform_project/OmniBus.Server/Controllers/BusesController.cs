using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models.Enums;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusesController : ControllerBase
    {
        private readonly IBusService _busService;
        public BusesController(IBusService busService) => _busService = busService;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() => Ok(await _busService.GetAllBusesAsync());

        [HttpGet("my")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetMy()
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _busService.GetBusesForOperatorAsync(opId));
        }

        [HttpPost]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> Create([FromBody] CreateBusRequest request)
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _busService.CreateBusAsync(request, opId));
        }

        [HttpPut("{busId}/toggle-status")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> ToggleStatus(Guid busId, [FromBody] BusStatusToggleRequest request)
        {
            var opId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _busService.ToggleStatusAsync(busId, request.Status, opId));
        }

        [HttpPut("{busId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid busId) => Ok(await _busService.ApproveBusAsync(busId));

        [HttpPut("{busId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid busId) => Ok(await _busService.RejectBusAsync(busId));
    }
}
