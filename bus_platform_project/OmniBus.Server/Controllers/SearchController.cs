using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        public SearchController(ISearchService searchService) => _searchService = searchService;

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? source, [FromQuery] string? destination, [FromQuery] DateTime? date) =>
            Ok(await _searchService.SearchBusesAsync(source, destination, date));
    }
}
