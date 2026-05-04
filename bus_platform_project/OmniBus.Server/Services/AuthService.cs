using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> SendOtpAsync(string email);
        Task<ApiResponse<AuthResponse>> VerifyOtpAsync(string email, string code);
    }

    public class AuthService : IAuthService
    {
        private readonly OmniBusDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(OmniBusDbContext db, IConfiguration config, IEmailService emailService, ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> SendOtpAsync(string email)
        {
            // Generate 6-digit OTP
            var code = Random.Shared.Next(100000, 999999).ToString();
            var otp = new OtpRecord
            {
                Email = email.ToLower().Trim(),
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            // Invalidate previous OTPs
            var oldOtps = await _db.OtpRecords
                .Where(o => o.Email == otp.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            foreach (var old in oldOtps) old.IsUsed = true;

            _db.OtpRecords.Add(otp);
            await _db.SaveChangesAsync();

            // Log to console for development visibility
            _logger.LogInformation(">>> DEVELOPMENT OTP for {Email}: {Code} <<<", email, code);

            // Send email
            await _emailService.SendOtpEmailAsync(email, code);

            return new ApiResponse<string>(true, "OTP sent successfully", email);
        }

        public async Task<ApiResponse<AuthResponse>> VerifyOtpAsync(string email, string code)
        {
            var normalizedEmail = email.ToLower().Trim();
            var otp = await _db.OtpRecords
                .Where(o => o.Email == normalizedEmail && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return new ApiResponse<AuthResponse>(false, "Invalid or expired OTP", null);

            otp.IsUsed = true;

            // Find or create user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                user = new User { Email = normalizedEmail, IsVerified = true, Role = UserRole.Customer };
                _db.Users.Add(user);
            }
            else
            {
                user.IsVerified = true;
            }

            await _db.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var response = new AuthResponse(token, user.Email, user.Role.ToString(), user.UserId, user.FullName);
            return new ApiResponse<AuthResponse>(true, "Login successful", response);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("fullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
