using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface ISearchService
    {
        Task<ApiResponse<List<SearchResultDto>>> SearchBusesAsync(string? source, string? destination, DateTime? date);
    }

    public class SearchService : ISearchService
    {
        private readonly OmniBusDbContext _db;
        public SearchService(OmniBusDbContext db) => _db = db;

        public async Task<ApiResponse<List<SearchResultDto>>> SearchBusesAsync(string? source, string? destination, DateTime? date)
        {
            var now = DateTime.UtcNow;
            var query = _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Operator).ThenInclude(o => o.OperatorProfile)
                .Include(b => b.Seats)
                .Where(b => b.Status == BusStatus.Active && b.DepartureTime >= now);

            if (!string.IsNullOrWhiteSpace(source))
            {
                query = query.Where(b => EF.Functions.ILike(b.Route.SourceCity, $"%{source}%"));
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(b => EF.Functions.ILike(b.Route.DestinationCity, $"%{destination}%"));
            }

            if (date.HasValue)
            {
                var start = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                query = query.Where(b => b.DepartureTime >= start);
            }

            var buses = await query.OrderBy(b => b.DepartureTime).ToListAsync();

            // Calculate available seats per bus
            var busIds = buses.Select(b => b.BusId).ToList();
            var bookedSeatCounts = await _db.BookingSeats
                .Where(bs => busIds.Contains(bs.Seat.BusId) && bs.Booking.Status == BookingStatus.Paid)
                .GroupBy(bs => bs.Seat.BusId)
                .Select(g => new { BusId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BusId, x => x.Count);

            var results = buses.Select(b =>
            {
                var booked = bookedSeatCounts.GetValueOrDefault(b.BusId, 0);
                return new SearchResultDto(
                    b.BusId, b.Route.SourceCity, b.Route.DestinationCity, b.BusNumber,
                    b.Operator.OperatorProfile?.BusinessName ?? b.Operator.FullName, b.BasePrice, b.TotalSeats - booked, b.TotalSeats,
                    b.PickupAddress, b.DropoffAddress, b.DepartureTime);
            }).ToList();

            return new ApiResponse<List<SearchResultDto>>(true, $"Found {results.Count} buses", results);
        }
    }
}
