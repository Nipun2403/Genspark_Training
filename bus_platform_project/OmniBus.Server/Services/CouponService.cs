using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;

namespace OmniBus.Server.Services
{
    public interface ICouponService
    {
        Task<ApiResponse<CouponDto>> ValidateCouponAsync(string code);
        Task<ApiResponse<CouponDto>> GenerateCouponAsync(decimal discountPercent, Guid? userId);
        Task MarkCouponUsedAsync(string code);
    }

    public class CouponService : ICouponService
    {
        private readonly OmniBusDbContext _db;
        public CouponService(OmniBusDbContext db) => _db = db;

        public async Task<ApiResponse<CouponDto>> ValidateCouponAsync(string code)
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon == null) return new ApiResponse<CouponDto>(false, "Invalid coupon", null);
            var isValid = !coupon.IsUsed && coupon.ExpiryDate > DateTime.UtcNow;
            return new ApiResponse<CouponDto>(true, "OK",
                new CouponDto(coupon.Code, coupon.DiscountPercent, coupon.ExpiryDate, isValid));
        }

        public async Task<ApiResponse<CouponDto>> GenerateCouponAsync(decimal discountPercent, Guid? userId)
        {
            var code = $"OMNI-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var coupon = new Coupon
            {
                Code = code, DiscountPercent = discountPercent,
                ExpiryDate = DateTime.UtcNow.AddDays(90), UserId = userId
            };
            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();
            return new ApiResponse<CouponDto>(true, "Coupon generated",
                new CouponDto(code, discountPercent, coupon.ExpiryDate, true));
        }

        public async Task MarkCouponUsedAsync(string code)
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon != null) { coupon.IsUsed = true; await _db.SaveChangesAsync(); }
        }
    }
}
