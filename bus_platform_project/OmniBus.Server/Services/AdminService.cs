using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IAdminService
    {
        Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync();
        Task<ApiResponse<List<OperatorProfileDto>>> GetPendingOperatorsAsync();
        Task<ApiResponse<OperatorProfileDto>> ApproveRejectOperatorAsync(Guid userId, bool approve, string? reason);
        Task<ApiResponse<ToggleOperatorResponse>> ToggleOperatorStatusAsync(Guid operatorUserId, bool enable);
        Task<ApiResponse<AdminRevenueAnalyticsDto>> GetRevenueAnalyticsAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly OmniBusDbContext _db;
        private readonly ICouponService _couponService;
        private readonly IEmailService _emailService;

        public AdminService(OmniBusDbContext db, ICouponService couponService, IEmailService emailService)
        { _db = db; _couponService = couponService; _emailService = emailService; }

        public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync()
        {
            var totalBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Paid);
            var totalRevenue = await _db.Bookings.Where(b => b.Status == BookingStatus.Paid).SumAsync(b => (decimal?)b.TotalAmount) ?? 0;
            var activeOps = await _db.OperatorProfiles.CountAsync(o => o.ApprovalStatus == ApprovalStatus.Approved);
            var pendingOps = await _db.OperatorProfiles.CountAsync(o => o.ApprovalStatus == ApprovalStatus.Pending);
            var totalRoutes = await _db.Routes.CountAsync(r => r.IsActive);
            var activeBuses = await _db.Buses.CountAsync(b => b.Status == BusStatus.Active);

            return new ApiResponse<AdminDashboardDto>(true, "OK",
                new AdminDashboardDto(totalBookings, totalRevenue, totalRevenue * 0.10m, activeOps, pendingOps, totalRoutes, activeBuses));
        }

        public async Task<ApiResponse<List<OperatorProfileDto>>> GetPendingOperatorsAsync()
        {
            var ops = await _db.OperatorProfiles.Include(o => o.User)
                .Select(o => new OperatorProfileDto(o.ProfileId, o.UserId, o.User.Email, o.BusinessName,
                    o.ContactDetails, o.ApprovalStatus.ToString(), o.RejectionReason, o.CreatedAt))
                .ToListAsync();
            return new ApiResponse<List<OperatorProfileDto>>(true, "OK", ops);
        }

        public async Task<ApiResponse<OperatorProfileDto>> ApproveRejectOperatorAsync(Guid userId, bool approve, string? reason)
        {
            var profile = await _db.OperatorProfiles.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == userId);
            if (profile == null) return new ApiResponse<OperatorProfileDto>(false, "Operator not found", null);

            profile.ApprovalStatus = approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            profile.RejectionReason = reason;
            profile.ReviewedAt = DateTime.UtcNow;

            if (approve) profile.User.Role = UserRole.Operator;
            await _db.SaveChangesAsync();

            await _emailService.SendOperatorStatusEmailAsync(profile.User.Email, approve ? "Approved" : "Rejected", reason);

            return new ApiResponse<OperatorProfileDto>(true, $"Operator {(approve ? "approved" : "rejected")}",
                new OperatorProfileDto(profile.ProfileId, profile.UserId, profile.User.Email, profile.BusinessName,
                    profile.ContactDetails, profile.ApprovalStatus.ToString(), profile.RejectionReason, profile.CreatedAt));
        }

        public async Task<ApiResponse<ToggleOperatorResponse>> ToggleOperatorStatusAsync(Guid operatorUserId, bool enable)
        {
            var profile = await _db.OperatorProfiles.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == operatorUserId);
            if (profile == null) return new ApiResponse<ToggleOperatorResponse>(false, "Operator not found", null);

            if (enable)
            {
                profile.ApprovalStatus = ApprovalStatus.Approved;
                profile.User.Role = UserRole.Operator;
                
                // Re-activate buses (only if they were auto-disabled)
                var disabledBuses = await _db.Buses.Where(b => b.OperatorId == operatorUserId && b.Status == BusStatus.Disabled).ToListAsync();
                foreach (var bus in disabledBuses) bus.Status = BusStatus.Active;

                await _db.SaveChangesAsync();
                await _emailService.SendOperatorStatusEmailAsync(profile.User.Email, "Reactivated", "Your operator account has been reactivated.");

                return new ApiResponse<ToggleOperatorResponse>(true, "Operator reactivated", 
                    new ToggleOperatorResponse(0, 0, "Operator and associated buses reactivated."));
            }
            else
            {
                profile.ApprovalStatus = ApprovalStatus.Disabled;
                
                // Disable all buses
                var buses = await _db.Buses.Where(b => b.OperatorId == operatorUserId).ToListAsync();
                foreach (var bus in buses) bus.Status = BusStatus.Disabled;

                // Find and refund future bookings
                var busIds = buses.Select(b => b.BusId).ToList();
                var futureBookings = await _db.Bookings
                    .Include(b => b.User).Include(b => b.Bus).ThenInclude(b => b.Route)
                    .Where(b => busIds.Contains(b.BusId) && b.Status == BookingStatus.Paid && b.Bus.DepartureTime > DateTime.UtcNow)
                    .ToListAsync();

                int couponsGenerated = 0;
                foreach (var booking in futureBookings)
                {
                    booking.Status = BookingStatus.Refunded;
                    booking.RefundAmount = booking.TotalAmount;

                    var coupon = await _couponService.GenerateCouponAsync(10, booking.UserId);
                    couponsGenerated++;

                    var busDetails = $"{booking.Bus.BusNumber}: {booking.Bus.Route.SourceCity} → {booking.Bus.Route.DestinationCity} on {booking.Bus.DepartureTime:dd MMM yyyy}";
                    var searchUrl = $"http://localhost:4200/search?source={Uri.EscapeDataString(booking.Bus.Route.SourceCity)}&destination={Uri.EscapeDataString(booking.Bus.Route.DestinationCity)}&date={booking.Bus.DepartureTime:yyyy-MM-dd}";
                    
                    await _emailService.SendCancellationEmailAsync(booking.User.Email, busDetails,
                        "Full refund processed due to operator maintenance.", coupon.Data?.Code, searchUrl);
                }

                await _db.SaveChangesAsync();
                await _emailService.SendOperatorStatusEmailAsync(profile.User.Email, "Disabled", "Your account has been disabled by admin.");

                return new ApiResponse<ToggleOperatorResponse>(true, "Operator disabled",
                    new ToggleOperatorResponse(futureBookings.Count, couponsGenerated,
                        $"Operator disabled. {futureBookings.Count} bookings refunded, {couponsGenerated} coupons generated."));
            }
        }

        public async Task<ApiResponse<AdminRevenueAnalyticsDto>> GetRevenueAnalyticsAsync()
        {
            // Load paid bookings with related data into memory for grouping
            var paidBookings = await _db.Bookings
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .Include(b => b.Bus).ThenInclude(b => b.Operator)
                .Where(b => b.Status == BookingStatus.Paid)
                .ToListAsync();

            var routeRevenue = paidBookings
                .GroupBy(b => $"{b.Bus.Route.SourceCity} → {b.Bus.Route.DestinationCity}")
                .Select(g => new RouteRevenueDto(g.Key, g.Sum(b => b.TotalAmount), g.Count()))
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var operatorRevenue = paidBookings
                .GroupBy(b => b.Bus.Operator?.FullName ?? "Unknown")
                .Select(g => new OperatorRevenueDto(
                    g.Key,
                    g.Sum(b => b.TotalAmount),
                    g.Sum(b => b.AdminCommission),
                    _db.Buses.Count(bus => bus.Operator != null && bus.Operator.FullName == g.Key)
                ))
                .OrderByDescending(x => x.TotalTurnover)
                .ToList();

            return new ApiResponse<AdminRevenueAnalyticsDto>(true, "OK", new AdminRevenueAnalyticsDto(routeRevenue, operatorRevenue));
        }
    }
}
