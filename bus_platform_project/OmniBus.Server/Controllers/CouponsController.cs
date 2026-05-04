using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;
        public CouponsController(ICouponService couponService) => _couponService = couponService;

        [HttpGet("validate/{code}")]
        [Authorize]
        public async Task<IActionResult> Validate(string code) =>
            Ok(await _couponService.ValidateCouponAsync(code));
    }
}
