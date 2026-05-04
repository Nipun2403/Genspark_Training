using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface ISeatService
    {
        Task<ApiResponse<List<SeatDto>>> GetSeatMapAsync(Guid busId);
        Task<ApiResponse<LockSeatResponse>> LockSeatsAsync(Guid busId, List<int> seatNumbers, Guid userId);
        Task<ApiResponse<bool>> ReleaseLockAsync(Guid lockId, Guid userId);
        Task ReleaseExpiredLocksAsync();
    }

    public class SeatService : ISeatService
    {
        private readonly OmniBusDbContext _db;
        private readonly ILogger<SeatService> _logger;

        public SeatService(OmniBusDbContext db, ILogger<SeatService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ApiResponse<List<SeatDto>>> GetSeatMapAsync(Guid busId)
        {
            var bus = await _db.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.BusId == busId);
            if (bus == null) return new ApiResponse<List<SeatDto>>(false, "Bus not found", null);

            var seatIds = bus.Seats.Select(s => s.SeatId).ToList();
            var activeLocks = await _db.SeatLocks.Where(sl => seatIds.Contains(sl.SeatId) && sl.ExpiresAt > DateTime.UtcNow).Select(sl => sl.SeatId).ToListAsync();
            var bookedSeats = await _db.BookingSeats.Where(bs => seatIds.Contains(bs.SeatId) && bs.Booking.Status == BookingStatus.Paid).Select(bs => bs.SeatId).ToListAsync();

            var seatDtos = bus.Seats.OrderBy(s => s.SeatNumber).Select(s =>
            {
                var status = bookedSeats.Contains(s.SeatId) ? "Booked" : activeLocks.Contains(s.SeatId) ? "Locked" : "Available";
                return new SeatDto(s.SeatId, s.SeatNumber, status);
            }).ToList();

            return new ApiResponse<List<SeatDto>>(true, "Seat map retrieved", seatDtos);
        }

        public async Task<ApiResponse<LockSeatResponse>> LockSeatsAsync(Guid busId, List<int> seatNumbers, Guid userId)
        {
            if (seatNumbers.Count > 5) return new ApiResponse<LockSeatResponse>(false, "Maximum 5 seats per transaction", null);

            using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var seats = await _db.Seats.Where(s => s.BusId == busId && seatNumbers.Contains(s.SeatNumber)).ToListAsync();
                if (seats.Count != seatNumbers.Count) { await tx.RollbackAsync(); return new ApiResponse<LockSeatResponse>(false, "Seats not found", null); }

                var seatIds = seats.Select(s => s.SeatId).ToList();
                if (await _db.SeatLocks.AnyAsync(sl => seatIds.Contains(sl.SeatId) && sl.ExpiresAt > DateTime.UtcNow)) { await tx.RollbackAsync(); return new ApiResponse<LockSeatResponse>(false, "Seats locked by another user", null); }
                if (await _db.BookingSeats.AnyAsync(bs => seatIds.Contains(bs.SeatId) && bs.Booking.Status == BookingStatus.Paid)) { await tx.RollbackAsync(); return new ApiResponse<LockSeatResponse>(false, "Seats already booked", null); }

                var expiresAt = DateTime.UtcNow.AddMinutes(5);
                var locks = seats.Select(s => new SeatLock { SeatId = s.SeatId, UserId = userId, ExpiresAt = expiresAt }).ToList();
                _db.SeatLocks.AddRange(locks);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return new ApiResponse<LockSeatResponse>(true, "Seats locked", new LockSeatResponse(locks.Select(l => l.LockId).ToList(), expiresAt));
            }
            catch { await tx.RollbackAsync(); throw; }
        }

        public async Task<ApiResponse<bool>> ReleaseLockAsync(Guid lockId, Guid userId)
        {
            var sl = await _db.SeatLocks.FirstOrDefaultAsync(s => s.LockId == lockId && s.UserId == userId);
            if (sl == null) return new ApiResponse<bool>(false, "Lock not found", false);
            _db.SeatLocks.Remove(sl);
            await _db.SaveChangesAsync();
            return new ApiResponse<bool>(true, "Lock released", true);
        }

        public async Task ReleaseExpiredLocksAsync()
        {
            var expired = await _db.SeatLocks.Where(sl => sl.ExpiresAt <= DateTime.UtcNow).ToListAsync();
            if (expired.Any()) { _db.SeatLocks.RemoveRange(expired); await _db.SaveChangesAsync(); _logger.LogInformation("Released {Count} expired locks", expired.Count); }
        }
    }
}
