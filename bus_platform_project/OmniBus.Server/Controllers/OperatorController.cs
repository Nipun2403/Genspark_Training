using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OperatorController : ControllerBase
    {
        private readonly OmniBusDbContext _db;
        public OperatorController(OmniBusDbContext db) => _db = db;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] OperatorRegisterRequest request)
        {
            var user = await _db.Users.FindAsync(UserId);
            if (user == null) return NotFound();

            var existing = await _db.OperatorProfiles.AnyAsync(o => o.UserId == UserId);
            if (existing) return BadRequest(new ApiResponse<string>(false, "Already registered as operator", null));

            user.FullName = request.FullName;
            user.Phone = request.Phone;

            var profile = new OperatorProfile
            {
                UserId = UserId, BusinessName = request.BusinessName,
                ContactDetails = request.ContactDetails, ApprovalStatus = ApprovalStatus.Pending
            };
            _db.OperatorProfiles.Add(profile);
            await _db.SaveChangesAsync();

            return Ok(new ApiResponse<OperatorProfileDto>(true, "Registration submitted",
                new OperatorProfileDto(profile.ProfileId, UserId, user.Email, profile.BusinessName,
                    profile.ContactDetails, "Pending", null, profile.CreatedAt)));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _db.OperatorProfiles.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == UserId);
            if (profile == null) return NotFound();
            return Ok(new ApiResponse<OperatorProfileDto>(true, "OK",
                new OperatorProfileDto(profile.ProfileId, UserId, profile.User.Email, profile.BusinessName,
                    profile.ContactDetails, profile.ApprovalStatus.ToString(), profile.RejectionReason, profile.CreatedAt)));
        }

        [HttpGet("manifest/{busId}")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetManifest(Guid busId)
        {
            var bus = await _db.Buses.Include(b => b.Route)
                .FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == UserId);
            if (bus == null) return NotFound();

            var passengers = await _db.BookingSeats
                .Include(bs => bs.Seat).Include(bs => bs.Booking)
                .Where(bs => bs.Seat.BusId == busId && bs.Booking.Status == BookingStatus.Paid)
                .OrderBy(bs => bs.Seat.SeatNumber)
                .Select(bs => new ManifestPassengerDto(bs.Seat.SeatNumber, bs.PassengerName, bs.PassengerAge,
                    bs.PassengerGender.ToString(), bs.PassengerMobile))
                .ToListAsync();

            var manifest = new ManifestDto(bus.BusNumber,
                $"{bus.Route.SourceCity} → {bus.Route.DestinationCity}", bus.DepartureTime, passengers);
            return Ok(new ApiResponse<ManifestDto>(true, "OK", manifest));
        }

        [HttpPatch("bus/{busId}")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> UpdateBus(Guid busId, [FromBody] CreateBusRequest request)
        {
            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.BusId == busId && b.OperatorId == UserId);
            if (bus == null) return NotFound();

            bus.PlateNumber = request.PlateNumber;
            bus.BusNumber = request.BusNumber;
            bus.PickupAddress = request.PickupAddress;
            bus.DropoffAddress = request.DropoffAddress;
            bus.DepartureTime = request.DepartureTime;
            bus.BasePrice = request.BasePrice;

            if (request.TotalSeats.HasValue && request.TotalSeats.Value != bus.TotalSeats)
            {
                var newTotal = request.TotalSeats.Value;
                if (newTotal > bus.TotalSeats)
                {
                    var newSeats = Enumerable.Range(bus.TotalSeats + 1, newTotal - bus.TotalSeats)
                        .Select(n => new Seat { BusId = busId, SeatNumber = n });
                    _db.Seats.AddRange(newSeats);
                }
                else
                {
                    var seatsToRemove = await _db.Seats
                        .Where(s => s.BusId == busId && s.SeatNumber > newTotal)
                        .ToListAsync();
                    
                    var seatIdsToRemove = seatsToRemove.Select(s => s.SeatId).ToList();
                    var hasBookings = await _db.BookingSeats.AnyAsync(bs => seatIdsToRemove.Contains(bs.SeatId));
                    
                    if (hasBookings)
                        return BadRequest(new ApiResponse<string>(false, "Cannot reduce seats: high-number seats already booked.", null));

                    _db.Seats.RemoveRange(seatsToRemove);
                }
                bus.TotalSeats = newTotal;
            }

            await _db.SaveChangesAsync();
            return Ok(new ApiResponse<string>(true, "Bus details updated", null));
        }

        [HttpGet("revenue")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetRevenue()
        {
            var busIds = await _db.Buses.Where(b => b.OperatorId == UserId).Select(b => b.BusId).ToListAsync();
            
            var bookings = await _db.Bookings
                .Include(b => b.Bus).ThenInclude(b => b.Route)
                .Include(b => b.BookingSeats)
                .Where(b => busIds.Contains(b.BusId) && b.Status == BookingStatus.Paid)
                .ToListAsync();

            var totalRevenue = bookings.Sum(b => b.TotalAmount);
            var totalEarnings = bookings.Sum(b => b.BaseFare);

            var perBus = bookings
                .GroupBy(b => b.BusId)
                .Select(g => {
                    var b = g.First().Bus;
                    return new BusRevenueDto(
                        g.Key,
                        b.BusNumber,
                        $"{b.Route.SourceCity} → {b.Route.DestinationCity}",
                        g.Sum(bk => bk.TotalAmount),
                        g.Sum(bk => bk.BookingSeats.Count)
                    );
                })
                .ToList();

            return Ok(new ApiResponse<OperatorRevenueAnalyticsDto>(true, "OK", new OperatorRevenueAnalyticsDto(totalRevenue, totalEarnings, perBus)));
        }
    }
}
