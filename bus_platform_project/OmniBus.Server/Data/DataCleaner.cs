using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OmniBus.Server.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OmniBus.Server.Scratch
{
    public class DataCleaner
    {
        public static async Task CleanAllData(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OmniBusDbContext>();

            Console.WriteLine("Cleaning all tables (Dropping for schema refresh)...");

            var tableNames = new[] 
            { 
                "BookingSeats", "Bookings", "SeatLocks", "Seats", 
                "Buses", "Routes", "OperatorProfiles", "OtpRecords", 
                "Coupons", "Users" 
            };

            foreach (var table in tableNames)
            {
                try
                {
                    // DROP tables to force schema recreation on restart
                    await db.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"" + table + "\" CASCADE;");
                    Console.WriteLine($"Dropped {table}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error dropping {table}: {ex.Message}");
                }
            }

            Console.WriteLine("Database schema cleared. Restart the app to re-create tables.");
        }
    }
}
