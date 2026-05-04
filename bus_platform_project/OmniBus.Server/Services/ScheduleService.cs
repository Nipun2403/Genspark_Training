using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IScheduleService
    {
        Task<ApiResponse<BusScheduleDto>> CreateScheduleAsync(CreateScheduleRequest req, Guid operatorId);
        Task<ApiResponse<List<BusScheduleDto>>> GetMySchedulesAsync(Guid operatorId);
        Task<ApiResponse<bool>> ToggleScheduleAsync(Guid scheduleId, bool isActive, Guid operatorId);
        Task<ApiResponse<bool>> DeleteScheduleAsync(Guid scheduleId, Guid operatorId);
        Task ProcessSchedulesJobAsync(); // Background job
    }

    public class ScheduleService : IScheduleService
    {
        private readonly OmniBusDbContext _db;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(OmniBusDbContext db, ILogger<ScheduleService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ApiResponse<BusScheduleDto>> CreateScheduleAsync(CreateScheduleRequest req, Guid operatorId)
        {
            var route = await _db.Routes.FindAsync(req.RouteId);
            if (route == null) return new ApiResponse<BusScheduleDto>(false, "Route not found", null);

            if (!TimeSpan.TryParse(req.DepartureTime, out var departureTime))
            {
                // Try alternate format if HH:mm fails
                if (!TimeSpan.TryParseExact(req.DepartureTime, new[] { "hh\\:mm", "h\\:mm", "hh\\:mm\\:ss" }, null, out departureTime))
                    return new ApiResponse<BusScheduleDto>(false, "Invalid time format. Use HH:mm", null);
            }

            var schedule = new BusSchedule
            {
                OperatorId = operatorId,
                RouteId = req.RouteId,
                PlateNumber = req.PlateNumber,
                BusNumber = req.BusNumber,
                BasePrice = req.BasePrice,
                TotalSeats = req.TotalSeats,
                PickupAddress = req.PickupAddress,
                DropoffAddress = req.DropoffAddress,
                DepartureTime = departureTime,
                IsActive = true
            };

            _db.BusSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            // Trigger an immediate run for today/tomorrow
            await ProcessSchedulesJobAsync();

            return new ApiResponse<BusScheduleDto>(true, "Schedule created", MapToDto(schedule, route));
        }

        public async Task<ApiResponse<List<BusScheduleDto>>> GetMySchedulesAsync(Guid operatorId)
        {
            var schedules = await _db.BusSchedules
                .Include(s => s.Route)
                .Where(s => s.OperatorId == operatorId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return new ApiResponse<List<BusScheduleDto>>(true, "OK", schedules.Select(s => MapToDto(s, s.Route)).ToList());
        }

        public async Task<ApiResponse<bool>> ToggleScheduleAsync(Guid scheduleId, bool isActive, Guid operatorId)
        {
            var schedule = await _db.BusSchedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.OperatorId == operatorId);
            if (schedule == null) return new ApiResponse<bool>(false, "Schedule not found", false);

            schedule.IsActive = isActive;
            await _db.SaveChangesAsync();
            
            if (isActive) await ProcessSchedulesJobAsync(); // Re-populate if resumed

            return new ApiResponse<bool>(true, $"Schedule {(isActive ? "started" : "stopped")}", true);
        }

        public async Task<ApiResponse<bool>> DeleteScheduleAsync(Guid scheduleId, Guid operatorId)
        {
            var schedule = await _db.BusSchedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.OperatorId == operatorId);
            if (schedule == null) return new ApiResponse<bool>(false, "Schedule not found", false);

            _db.BusSchedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return new ApiResponse<bool>(true, "Schedule deleted", true);
        }

        public async Task ProcessSchedulesJobAsync()
        {
            _logger.LogInformation("Processing recurring bus schedules at {Time}", DateTime.UtcNow);
            
            var activeSchedules = await _db.BusSchedules.Where(s => s.IsActive).ToListAsync();
            var targetDates = new[] { DateTime.Today, DateTime.Today.AddDays(1) };
            
            int createdCount = 0;
            foreach (var date in targetDates)
            {
                foreach (var schedule in activeSchedules)
                {
                    var departureTime = date.Add(schedule.DepartureTime);
                    
                    // Skip if the departure time has already passed today
                    if (departureTime < DateTime.Now) continue;

                    // Check if already created for this specific time to avoid duplicates
                    var exists = await _db.Buses.AnyAsync(b => 
                        b.OperatorId == schedule.OperatorId && 
                        b.BusNumber == schedule.BusNumber && 
                        b.DepartureTime == departureTime);

                    if (!exists)
                    {
                        var bus = new Bus
                        {
                            OperatorId = schedule.OperatorId,
                            RouteId = schedule.RouteId,
                            PlateNumber = schedule.PlateNumber,
                            BusNumber = schedule.BusNumber,
                            BasePrice = schedule.BasePrice,
                            TotalSeats = schedule.TotalSeats,
                            PickupAddress = schedule.PickupAddress,
                            DropoffAddress = schedule.DropoffAddress,
                            DepartureTime = departureTime,
                            Status = BusStatus.Active
                        };

                        _db.Buses.Add(bus);
                        await _db.SaveChangesAsync();

                        var seats = Enumerable.Range(1, schedule.TotalSeats)
                            .Select(n => new Seat { BusId = bus.BusId, SeatNumber = n })
                            .ToList();
                        
                        _db.Seats.AddRange(seats);
                        createdCount++;
                    }
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Schedule processing complete. Created {Count} new bus instances.", createdCount);
        }

        private BusScheduleDto MapToDto(BusSchedule s, Models.Route r) =>
            new(s.ScheduleId, s.RouteId, r.SourceCity, r.DestinationCity, s.PlateNumber, s.BusNumber, s.BasePrice, s.TotalSeats, s.PickupAddress, s.DropoffAddress, s.DepartureTime.ToString(@"hh\:mm"), s.IsActive);
    }
}
