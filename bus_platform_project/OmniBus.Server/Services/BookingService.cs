using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IBookingService
    {
        Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingRequest req, Guid userId);
        Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(Guid userId);
        Task<ApiResponse<CancelBookingResponse>> CancelBookingAsync(Guid bookingId, Guid userId);
        Task<ApiResponse<BookingDto>> GetBookingAsync(Guid bookingId, Guid userId);
    }

    public class BookingService : IBookingService
    {
        private readonly OmniBusDbContext _db;
        private readonly ICouponService _couponService;
        private readonly IEmailService _emailService;

        public BookingService(OmniBusDbContext db, ICouponService couponService, IEmailService emailService)
        { _db = db; _couponService = couponService; _emailService = emailService; }

        public async Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingRequest req, Guid userId)
        {
            if (req.Passengers.Count > 5)
                return new ApiResponse<BookingDto>(false, "Max 5 seats per booking", null);

            var bus = await _db.Buses.Include(b => b.Route).FirstOrDefaultAsync(b => b.BusId == req.BusId);
            if (bus == null) return new ApiResponse<BookingDto>(false, "Bus not found", null);

            // Verify all seats are locked by this user
            var seatIds = req.Passengers.Select(p => p.SeatId).ToList();
            var locks = await _db.SeatLocks.Where(sl => seatIds.Contains(sl.SeatId) && sl.UserId == userId && sl.ExpiresAt > DateTime.UtcNow).ToListAsync();
            if (locks.Count != seatIds.Count)
                return new ApiResponse<BookingDto>(false, "Seats not locked or locks expired", null);

            decimal discount = 0;
            if (!string.IsNullOrEmpty(req.CouponCode))
            {
                var couponResult = await _couponService.ValidateCouponAsync(req.CouponCode);
                if (couponResult.Success && couponResult.Data != null && couponResult.Data.IsValid)
                    discount = couponResult.Data.DiscountPercent;
            }

            var totalBaseFare = bus.BasePrice * req.Passengers.Count;
            var discountedBaseFare = totalBaseFare - (totalBaseFare * discount / 100);

            var gst = discountedBaseFare * 0.05m;
            var serviceFee = discountedBaseFare * 0.02m;
            var bookingFee = 50m;
            var adminCommission = gst + serviceFee + bookingFee;
            var totalFinal = discountedBaseFare + adminCommission;

            var booking = new Booking
            {
                UserId = userId, BusId = req.BusId, TotalAmount = totalFinal,
                BaseFare = discountedBaseFare, Gst = gst, ServiceFee = serviceFee,
                BookingFee = bookingFee, AdminCommission = adminCommission,
                Status = BookingStatus.Pending, CouponCode = req.CouponCode, DiscountPercent = discount
            };
            _db.Bookings.Add(booking);

            foreach (var p in req.Passengers)
            {
                _db.BookingSeats.Add(new BookingSeat
                {
                    BookingId = booking.BookingId, SeatId = p.SeatId,
                    PassengerName = p.Name, PassengerAge = p.Age,
                    PassengerGender = p.Gender, PassengerMobile = p.Mobile
                });
            }

            await _db.SaveChangesAsync();
            return new ApiResponse<BookingDto>(true, "Booking created", await MapToDto(booking.BookingId));
        }

        public async Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _db.Bookings.Where(b => b.UserId == userId)
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .Include(b => b.Bus).ThenInclude(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.CreatedAt).ToListAsync();

            return new ApiResponse<List<BookingDto>>(true, "OK", bookings.Select(MapFromEntity).ToList());
        }

        public async Task<ApiResponse<CancelBookingResponse>> CancelBookingAsync(Guid bookingId, Guid userId)
        {
            var booking = await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);
                
            if (booking == null) return new ApiResponse<CancelBookingResponse>(false, "Booking not found", null);
            if (booking.Status != BookingStatus.Paid) return new ApiResponse<CancelBookingResponse>(false, "Only paid bookings can be cancelled", null);

            var hoursUntilDeparture = (booking.Bus.DepartureTime - DateTime.UtcNow).TotalHours;
            decimal refundPercent = hoursUntilDeparture > 48 ? 100 : hoursUntilDeparture > 24 ? 50 : 0;
            var refundAmount = booking.TotalAmount * refundPercent / 100;

            booking.Status = BookingStatus.Cancelled;
            booking.RefundAmount = refundAmount;
            await _db.SaveChangesAsync();

            string? couponCode = null;
            if (refundPercent < 100)
            {
                var coupon = await _couponService.GenerateCouponAsync(10, userId);
                couponCode = coupon.Data?.Code;
            }

            // Send cancellation email (fire-and-forget)
            var busDetails = $"{booking.Bus.BusNumber}: {booking.Bus.Route.SourceCity} → {booking.Bus.Route.DestinationCity} on {booking.Bus.DepartureTime:dd MMM yyyy}";
            var refundStatus = refundPercent > 0 ? $"Refunded ₹{refundAmount:N2} ({refundPercent}%)" : "No refund (cancellation within 24 hours)";
            
            _ = Task.Run(async () =>
            {
                try
                {
                    var searchUrl = $"http://localhost:4200/search?source={Uri.EscapeDataString(booking.Bus.Route.SourceCity)}&destination={Uri.EscapeDataString(booking.Bus.Route.DestinationCity)}&date={booking.Bus.DepartureTime:yyyy-MM-dd}";
                    await _emailService.SendCancellationEmailAsync(booking.User.Email, busDetails, refundStatus, couponCode, searchUrl);
                }
                catch { /* ignore */ }
            });

            return new ApiResponse<CancelBookingResponse>(true, "Booking cancelled",
                new CancelBookingResponse(refundAmount, refundPercent, couponCode));
        }

        public async Task<ApiResponse<BookingDto>> GetBookingAsync(Guid bookingId, Guid userId)
        {
            var dto = await MapToDto(bookingId);
            if (dto == null) return new ApiResponse<BookingDto>(false, "Not found", null);
            return new ApiResponse<BookingDto>(true, "OK", dto);
        }

        private async Task<BookingDto?> MapToDto(Guid bookingId)
        {
            var b = await _db.Bookings
                .Include(x => x.Bus).ThenInclude(x => x.Route)
                .Include(x => x.Bus).ThenInclude(x => x.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(x => x.BookingSeats).ThenInclude(x => x.Seat)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);
            return b == null ? null : MapFromEntity(b);
        }

        private BookingDto MapFromEntity(Booking b) =>
            new(b.BookingId, b.BusId, b.Bus.Route.SourceCity, b.Bus.Route.DestinationCity,
                b.Bus.BusNumber, b.Bus.Operator?.OperatorProfile?.BusinessName ?? b.Bus.Operator?.FullName ?? "Unknown", 
                b.Bus.PickupAddress, b.Bus.DropoffAddress,
                b.TotalAmount, b.BaseFare, b.Gst, b.ServiceFee, b.BookingFee,
                b.DiscountPercent, b.Status.ToString(), b.Bus.DepartureTime, b.CreatedAt,
                b.BookingSeats.Select(bs => new BookingSeatDto(
                    bs.Seat.SeatNumber, bs.PassengerName, bs.PassengerAge,
                    bs.PassengerGender.ToString(), bs.PassengerMobile)).ToList(),
                b.CancellationNote);
    }
}
