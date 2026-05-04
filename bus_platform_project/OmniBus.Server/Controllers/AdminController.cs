using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService) => _adminService = adminService;

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard() => Ok(await _adminService.GetDashboardAsync());

        [HttpGet("operators")]
        public async Task<IActionResult> GetOperators() => Ok(await _adminService.GetPendingOperatorsAsync());

        [HttpPut("operators/{userId}/review")]
        public async Task<IActionResult> ReviewOperator(Guid userId, [FromBody] ApproveRejectRequest request) =>
            Ok(await _adminService.ApproveRejectOperatorAsync(userId, request.Approve, request.Reason));

        [HttpPut("operators/{userId}/toggle")]
        public async Task<IActionResult> ToggleOperator(Guid userId, [FromQuery] bool enable) =>
            Ok(await _adminService.ToggleOperatorStatusAsync(userId, enable));

        [HttpGet("revenue-analytics")]
        public async Task<IActionResult> RevenueAnalytics() => Ok(await _adminService.GetRevenueAnalyticsAsync());
    }
}
