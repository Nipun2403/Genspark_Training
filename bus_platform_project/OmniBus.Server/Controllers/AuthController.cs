using Microsoft.AspNetCore.Mvc;
using OmniBus.Server.DTOs;
using OmniBus.Server.Services;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            var result = await _authService.SendOtpAsync(request.Email);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _authService.VerifyOtpAsync(request.Email, request.Code);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
    }
}
