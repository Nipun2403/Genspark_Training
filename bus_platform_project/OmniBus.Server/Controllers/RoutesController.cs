using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        public RoutesController(IRouteService routeService) => _routeService = routeService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _routeService.GetAllRoutesAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateRouteRequest request)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _routeService.CreateRouteAsync(request, adminId));
        }

        [HttpDelete("{routeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid routeId) => Ok(await _routeService.DeleteRouteAsync(routeId));

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q, [FromQuery] string? fromCity = null, [FromQuery] string? toCity = null)
        {
            var list = await _routeService.GetCitySuggestionsAsync(q, fromCity, toCity);
            return Ok(new ApiResponse<List<string>>(true, "OK", list));
        }
    }
}
