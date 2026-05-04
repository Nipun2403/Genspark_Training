using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IBusService
    {
        Task<ApiResponse<BusDto>> CreateBusAsync(CreateBusRequest req, Guid operatorId);
        Task<ApiResponse<List<BusDto>>> GetBusesForOperatorAsync(Guid operatorId);
        Task<ApiResponse<BusDto>> ToggleStatusAsync(Guid busId, BusStatus status, Guid operatorId);
        Task<ApiResponse<List<BusDto>>> GetAllBusesAsync();
        Task<ApiResponse<BusDto>> ApproveBusAsync(Guid busId);
        Task<ApiResponse<BusDto>> RejectBusAsync(Guid busId);
    }

    public class BusService : IBusService
    {
        private readonly OmniBusDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ICouponService _couponService;

        public BusService(OmniBusDbContext db, IEmailService emailService, ICouponService couponService)
        { _db = db; _emailService = emailService; _couponService = couponService; }

        public async Task<ApiResponse<BusDto>> CreateBusAsync(CreateBusRequest req, Guid operatorId)
        {
            var route = await _db.Routes.FindAsync(req.RouteId);
            if (route == null) return new ApiResponse<BusDto>(false, "Route not found", null);

            var totalSeats = req.TotalSeats ?? 42;
            var bus = new Bus
            {
                OperatorId = operatorId, RouteId = req.RouteId, PlateNumber = req.PlateNumber,
                BusNumber = req.BusNumber, BasePrice = req.BasePrice, PickupAddress = req.PickupAddress,
                DropoffAddress = req.DropoffAddress, DepartureTime = req.DepartureTime,
                TotalSeats = totalSeats
            };
            _db.Buses.Add(bus);
            await _db.SaveChangesAsync();

            // Auto-create seats dynamically
            var seats = Enumerable.Range(1, totalSeats).Select(n => new Seat { BusId = bus.BusId, SeatNumber = n }).ToList();
            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();

            return new ApiResponse<BusDto>(true, "Bus created", MapToDto(bus, route, totalSeats));
        }

        public async Task<ApiResponse<List<BusDto>>> GetBusesForOperatorAsync(Guid operatorId)
        {
            var buses = await _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats).ThenInclude(s => s.BookingSeats).ThenInclude(bs => bs.Booking)
                .Where(b => b.OperatorId == operatorId).ToListAsync();
            return new ApiResponse<List<BusDto>>(true, "OK", buses.Select(b => MapToDto(b, b.Route, CountAvailable(b))).ToList());
        }

        public async Task<ApiResponse<BusDto>> ToggleStatusAsync(Guid busId, BusStatus status, Guid operatorId)
        {
            var bus = await _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats).ThenInclude(s => s.BookingSeats).ThenInclude(bs => bs.Booking)
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == operatorId);
            if (bus == null) return new ApiResponse<BusDto>(false, "Bus not found", null);
            
            var oldStatus = bus.Status;
            bus.Status = status;

            if (status == BusStatus.EmergencyOff && oldStatus != BusStatus.EmergencyOff)
            {
                // Handle emergency cancellations
                var futureBookings = await _db.Bookings
                    .Include(b => b.User)
                    .Where(b => b.BusId == busId && b.Status == BookingStatus.Paid)
                    .ToListAsync();

                var alternatives = await _db.Buses
                    .Where(b => b.RouteId == bus.RouteId && b.BusId != busId && b.Status == BusStatus.Active && b.DepartureTime > DateTime.UtcNow)
                    .Take(3)
                    .Select(b => $"{b.BusNumber} at {b.DepartureTime:hh:mm tt}")
                    .ToListAsync();

                var altText = alternatives.Any() ? "Suggested alternatives: " + string.Join(", ", alternatives) : "No immediate alternatives available.";

                foreach (var booking in futureBookings)
                {
                    booking.Status = BookingStatus.Refunded;
                    booking.RefundAmount = booking.TotalAmount;
                    booking.CancellationNote = altText;

                    var coupon = await _couponService.GenerateCouponAsync(15, booking.UserId);
                    var busDetails = $"{bus.BusNumber}: {bus.Route.SourceCity} → {bus.Route.DestinationCity} on {bus.DepartureTime:dd MMM yyyy}";
                    var searchUrl = $"http://localhost:4200/search?source={Uri.EscapeDataString(bus.Route.SourceCity)}&destination={Uri.EscapeDataString(bus.Route.DestinationCity)}&date={bus.DepartureTime:yyyy-MM-dd}";
                    
                    await _emailService.SendCancellationEmailAsync(booking.User.Email, busDetails, 
                        $"Emergency cancellation. Full refund + 15% discount coupon: {coupon.Data?.Code}. {altText}", 
                        coupon.Data?.Code, searchUrl);
                }
            }

            await _db.SaveChangesAsync();
            return new ApiResponse<BusDto>(true, "Status updated", MapToDto(bus, bus.Route, CountAvailable(bus)));
        }

        public async Task<ApiResponse<List<BusDto>>> GetAllBusesAsync()
        {
            var buses = await _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats).ThenInclude(s => s.BookingSeats).ThenInclude(bs => bs.Booking)
                .ToListAsync();
            return new ApiResponse<List<BusDto>>(true, "OK", buses.Select(b => MapToDto(b, b.Route, CountAvailable(b))).ToList());
        }

        public async Task<ApiResponse<BusDto>> ApproveBusAsync(Guid busId)
        {
            var bus = await _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats).ThenInclude(s => s.BookingSeats).ThenInclude(bs => bs.Booking)
                .FirstOrDefaultAsync(b => b.BusId == busId);
            if (bus == null) return new ApiResponse<BusDto>(false, "Bus not found", null);
            bus.Status = BusStatus.Active;
            await _db.SaveChangesAsync();
            return new ApiResponse<BusDto>(true, "Bus approved", MapToDto(bus, bus.Route, CountAvailable(bus)));
        }

        public async Task<ApiResponse<BusDto>> RejectBusAsync(Guid busId)
        {
            var bus = await _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats).ThenInclude(s => s.BookingSeats).ThenInclude(bs => bs.Booking)
                .FirstOrDefaultAsync(b => b.BusId == busId);
            if (bus == null) return new ApiResponse<BusDto>(false, "Bus not found", null);
            bus.Status = BusStatus.Rejected;
            await _db.SaveChangesAsync();
            return new ApiResponse<BusDto>(true, "Bus rejected", MapToDto(bus, bus.Route, CountAvailable(bus)));
        }

        private int CountAvailable(Bus bus)
        {
            if (bus.Seats == null) return bus.TotalSeats;
            return bus.Seats.Count(s => !s.BookingSeats.Any(bs => bs.Booking.Status == BookingStatus.Paid));
        }

        private BusDto MapToDto(Bus b, Models.Route r, int avail) =>
            new(b.BusId, b.OperatorId, b.Operator?.OperatorProfile?.BusinessName ?? b.Operator?.FullName ?? "Unknown", b.RouteId,
                r.SourceCity, r.DestinationCity, b.PlateNumber, b.BusNumber,
                b.BasePrice, b.TotalSeats, avail, b.Status.ToString(),
                b.PickupAddress, b.DropoffAddress, b.DepartureTime);
    }
}
