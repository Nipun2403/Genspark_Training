using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentResponse>> ProcessPaymentAsync(Guid bookingId, bool isSuccess, Guid userId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly OmniBusDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly ICouponService _couponService;

        public PaymentService(OmniBusDbContext db, IEmailService emailService, IPdfService pdfService, ICouponService couponService)
        { _db = db; _emailService = emailService; _pdfService = pdfService; _couponService = couponService; }

        public async Task<ApiResponse<PaymentResponse>> ProcessPaymentAsync(Guid bookingId, bool isSuccess, Guid userId)
        {
            var booking = await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null) return new ApiResponse<PaymentResponse>(false, "Booking not found", null);
            if (booking.Status != BookingStatus.Pending) return new ApiResponse<PaymentResponse>(false, "Booking already processed", null);

            if (!isSuccess)
            {
                // Release locks
                var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
                var locks = await _db.SeatLocks.Where(sl => seatIds.Contains(sl.SeatId) && sl.UserId == userId).ToListAsync();
                _db.SeatLocks.RemoveRange(locks);
                _db.Bookings.Remove(booking);
                await _db.SaveChangesAsync();
                return new ApiResponse<PaymentResponse>(true, "Payment failed",
                    new PaymentResponse(false, null, "Payment was declined. Seats released."));
            }

            // Success: convert to paid
            var paymentRef = $"PAY-{Guid.NewGuid().ToString()[..12].ToUpper()}";
            booking.Status = BookingStatus.Paid;
            booking.PaymentRef = paymentRef;

            // Remove seat locks
            var bookingSeatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
            var seatLocks = await _db.SeatLocks.Where(sl => bookingSeatIds.Contains(sl.SeatId)).ToListAsync();
            _db.SeatLocks.RemoveRange(seatLocks);

            // Mark coupon as used
            if (!string.IsNullOrEmpty(booking.CouponCode))
                await _couponService.MarkCouponUsedAsync(booking.CouponCode);

            await _db.SaveChangesAsync();

            // Generate PDF and send email (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    var pdf = _pdfService.GenerateTicketPdf(booking);
                    await _emailService.SendTicketEmailAsync(booking.User.Email, booking.User.FullName, pdf);
                }
                catch { /* logged in email service */ }
            });

            return new ApiResponse<PaymentResponse>(true, "Payment successful",
                new PaymentResponse(true, paymentRef, "Booking confirmed! Ticket sent to email."));
        }
    }
}
