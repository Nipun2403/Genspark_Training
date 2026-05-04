using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Data;
using OmniBus.Server.DTOs;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Services
{
    public interface IRouteService
    {
        Task<ApiResponse<List<RouteDto>>> GetAllRoutesAsync();
        Task<ApiResponse<RouteDto>> CreateRouteAsync(CreateRouteRequest request, Guid adminId);
        Task<ApiResponse<bool>> DeleteRouteAsync(Guid routeId);
        Task<List<RouteDto>> FuzzySearchCitiesAsync(string? source, string? destination);
        Task<List<string>> GetCitySuggestionsAsync(string query, string? fromCity = null, string? toCity = null);
    }

    public class RouteService : IRouteService
    {
        private readonly OmniBusDbContext _db;

        public RouteService(OmniBusDbContext db) => _db = db;

        public async Task<ApiResponse<List<RouteDto>>> GetAllRoutesAsync()
        {
            var routes = await _db.Routes
                .Include(r => r.Buses)
                .Select(r => new RouteDto(r.RouteId, r.SourceCity, r.DestinationCity, r.IsActive, r.Buses.Count(b => b.Status == BusStatus.Active)))
                .ToListAsync();
            return new ApiResponse<List<RouteDto>>(true, "Routes retrieved", routes);
        }

        public async Task<ApiResponse<RouteDto>> CreateRouteAsync(CreateRouteRequest request, Guid adminId)
        {
            var exists = await _db.Routes.AnyAsync(r =>
                r.SourceCity.ToLower() == request.SourceCity.ToLower() &&
                r.DestinationCity.ToLower() == request.DestinationCity.ToLower());

            if (exists)
                return new ApiResponse<RouteDto>(false, "Route already exists", null);

            var route = new Models.Route
            {
                SourceCity = request.SourceCity.Trim(),
                DestinationCity = request.DestinationCity.Trim(),
                CreatedBy = adminId
            };

            _db.Routes.Add(route);
            
            // Auto-create reverse route if it doesn't exist
            var reverseExists = await _db.Routes.AnyAsync(r =>
                r.SourceCity.ToLower() == request.DestinationCity.ToLower() &&
                r.DestinationCity.ToLower() == request.SourceCity.ToLower());
            
            if (!reverseExists)
            {
                _db.Routes.Add(new Models.Route
                {
                    SourceCity = request.DestinationCity.Trim(),
                    DestinationCity = request.SourceCity.Trim(),
                    CreatedBy = adminId
                });
            }

            await _db.SaveChangesAsync();

            return new ApiResponse<RouteDto>(true, "Route (and reverse) created",
                new RouteDto(route.RouteId, route.SourceCity, route.DestinationCity, route.IsActive, 0));
        }

        public async Task<ApiResponse<bool>> DeleteRouteAsync(Guid routeId)
        {
            var route = await _db.Routes.Include(r => r.Buses).FirstOrDefaultAsync(r => r.RouteId == routeId);
            if (route == null) return new ApiResponse<bool>(false, "Route not found", false);
            
            if (route.Buses.Any())
            {
                // Instead of failing, disable the route and all its buses
                route.IsActive = false;
                foreach (var bus in route.Buses)
                {
                    bus.Status = BusStatus.Disabled;
                }
                await _db.SaveChangesAsync();
                return new ApiResponse<bool>(true, "Route and associated buses disabled (deleted from search)", true);
            }

            _db.Routes.Remove(route);
            await _db.SaveChangesAsync();
            return new ApiResponse<bool>(true, "Route deleted", true);
        }

        public async Task<List<RouteDto>> FuzzySearchCitiesAsync(string? source, string? destination)
        {
            var query = _db.Routes.Where(r => r.IsActive);

            if (!string.IsNullOrWhiteSpace(source))
            {
                query = query.Where(r =>
                    EF.Functions.ILike(r.SourceCity, $"%{source}%") ||
                    EF.Functions.TrigramsAreSimilar(r.SourceCity, source));
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                query = query.Where(r =>
                    EF.Functions.ILike(r.DestinationCity, $"%{destination}%") ||
                    EF.Functions.TrigramsAreSimilar(r.DestinationCity, destination));
            }

            return await query
                .Include(r => r.Buses)
                .Select(r => new RouteDto(r.RouteId, r.SourceCity, r.DestinationCity, r.IsActive, r.Buses.Count(b => b.Status == BusStatus.Active)))
                .ToListAsync();
        }

        public async Task<List<string>> GetCitySuggestionsAsync(string query, string? fromCity = null, string? toCity = null)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<string>();
            query = query.ToLower();
            
            var queryable = _db.Routes
                .Where(r => r.IsActive && r.Buses.Any(b => b.Status == BusStatus.Active));

            if (!string.IsNullOrEmpty(fromCity))
            {
                // We have a source, suggest reachable destinations matching query
                return await queryable
                    .Where(r => r.SourceCity.ToLower() == fromCity.ToLower() && r.DestinationCity.ToLower().Contains(query))
                    .Select(r => r.DestinationCity)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();
            }
            
            if (!string.IsNullOrEmpty(toCity))
            {
                // We have a destination, suggest reachable sources matching query
                return await queryable
                    .Where(r => r.DestinationCity.ToLower() == toCity.ToLower() && r.SourceCity.ToLower().Contains(query))
                    .Select(r => r.SourceCity)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();
            }

            // Default: find any city that acts as a source or destination for an active bus
            var sources = await queryable
                .Where(r => r.SourceCity.ToLower().Contains(query))
                .Select(r => r.SourceCity)
                .ToListAsync();
                
            var destinations = await queryable
                .Where(r => r.DestinationCity.ToLower().Contains(query))
                .Select(r => r.DestinationCity)
                .ToListAsync();
            
            return sources.Concat(destinations).Distinct().Take(10).ToList();
        }
    }
}
