using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OmniBus.Server.Data;
using OmniBus.Server.Services;

namespace OmniBus.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──
            builder.Services.AddDbContext<OmniBusDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── Hangfire ──
            builder.Services.AddHangfire(config =>
                config.UsePostgreSqlStorage(c =>
                    c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
            builder.Services.AddHangfireServer();

            // ── JWT Authentication ──
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });
            builder.Services.AddAuthorization();

            // ── Services DI ──
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IRouteService, RouteService>();
            builder.Services.AddScoped<IBusService, BusService>();
            builder.Services.AddScoped<ISeatService, SeatService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddScoped<IScheduleService, ScheduleService>();

            // ── Controllers + Swagger ──
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OmniBus API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ── CORS ──
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            });

            var app = builder.Build();

            // ── Middleware ──
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // ── Hangfire Dashboard ──
            app.MapHangfireDashboard("/hangfire");

            // ── Recurring Jobs ──
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OmniBusDbContext>();

                // ── One-Time Dense Seeding Check ──
                var seedFlagPath = Path.Combine(Directory.GetCurrentDirectory(), ".seeded");
                if (!File.Exists(seedFlagPath))
                {
                    Console.WriteLine(">>>> PERFORMING ONE-TIME DENSE SEEDING <<<<");
                    await OmniBus.Server.Scratch.DataCleaner.CleanAllData(scope.ServiceProvider);
                    
                    var databaseCreator = db.GetService<IDatabaseCreator>() as IRelationalDatabaseCreator;
                    try { databaseCreator?.CreateTables(); } catch { }

                    // 1. Seed Admins
                    var admin = new OmniBus.Server.Models.User { Email = "nipun620k+admin@gmail.com", Role = OmniBus.Server.Models.Enums.UserRole.Admin, FullName = "System Admin" };
                    var customer = new OmniBus.Server.Models.User { Email = "nipun620k+customer@gmail.com", Role = OmniBus.Server.Models.Enums.UserRole.Customer, FullName = "Regular Traveler" };
                    db.Users.AddRange(admin, customer);
                    await db.SaveChangesAsync();

                    // 2. Seed 10 Operators with Professional Names
                    var opData = new[] {
                        ("Genspark Elite", "operator1"), ("Rapid Transit", "operator2"), ("Blue Sky Travels", "operator3"),
                        ("City Express", "operator4"), ("Highway Kings", "operator5"), ("Comfort Coaches", "operator6"),
                        ("North Star", "operator7"), ("Oceanic Bus", "operator8"), ("Safe Journey", "operator9"), ("Apex Travels", "operator10")
                    };

                    foreach (var (name, suffix) in opData)
                    {
                        var email = $"nipun620k+{suffix}@gmail.com";
                        var user = new OmniBus.Server.Models.User { Email = email, Role = OmniBus.Server.Models.Enums.UserRole.Operator, FullName = name };
                        db.Users.Add(user);
                        await db.SaveChangesAsync();

                        db.OperatorProfiles.Add(new OmniBus.Server.Models.OperatorProfile {
                            UserId = user.UserId, BusinessName = name + " Ltd.", ContactDetails = $"Official: {email}",
                            ApprovalStatus = OmniBus.Server.Models.Enums.ApprovalStatus.Approved
                        });
                    }
                    await db.SaveChangesAsync();

                    // 3. Seed Routes
                    var routePairs = new[] { ("Delhi", "Agra"), ("Delhi", "Jaipur"), ("Delhi", "Chandigarh"), ("Mumbai", "Pune"), ("Mumbai", "Goa"), ("Bangalore", "Hyderabad"), ("Bangalore", "Goa"), ("Chennai", "Pondicherry") };
                    foreach (var (src, dst) in routePairs) {
                        db.Routes.Add(new OmniBus.Server.Models.Route { SourceCity = src, DestinationCity = dst, CreatedBy = admin.UserId });
                        db.Routes.Add(new OmniBus.Server.Models.Route { SourceCity = dst, DestinationCity = src, CreatedBy = admin.UserId });
                    }
                    await db.SaveChangesAsync();

                    // 4. Dense Bus Seeding (Multiple operators on same routes)
                    var allOps = db.Users.Where(u => u.Role == OmniBus.Server.Models.Enums.UserRole.Operator).ToList();
                    var allRoutes = db.Routes.ToList();
                    var plateIdx = 1000;

                    foreach (var route in allRoutes)
                    {
                        // Assign 2-3 different operators to EVERY route for density
                        var selectedOps = allOps.OrderBy(x => Guid.NewGuid()).Take(Random.Shared.Next(2, 4)).ToList();
                        foreach (var op in selectedOps)
                        {
                            // 1-2 buses per operator on this route at different times
                            var busCount = Random.Shared.Next(1, 3);
                            for (int i = 0; i < busCount; i++)
                            {
                                var seatCount = 40; // Changed to 40 to test dynamic persistence
                                var bus = new OmniBus.Server.Models.Bus {
                                    OperatorId = op.UserId, RouteId = route.RouteId,
                                    PlateNumber = $"DL-01-BK-{plateIdx++}",
                                    BusNumber = $"{op.FullName[..3].ToUpper()}-{plateIdx}",
                                    BasePrice = 450 + Random.Shared.Next(50, 600),
                                    PickupAddress = "Main Terminal, Platform " + Random.Shared.Next(1, 10),
                                    DropoffAddress = "City Center Mall St.",
                                    DepartureTime = DateTime.UtcNow.AddHours(Random.Shared.Next(12, 720)),
                                    Status = OmniBus.Server.Models.Enums.BusStatus.Active,
                                    TotalSeats = seatCount
                                };
                                db.Buses.Add(bus);
                                await db.SaveChangesAsync();
 
                                var seats = Enumerable.Range(1, seatCount).Select(n => new OmniBus.Server.Models.Seat { BusId = bus.BusId, SeatNumber = n }).ToList();
                                db.Seats.AddRange(seats);
                                await db.SaveChangesAsync();
                            }
                        }
                    }

                    File.WriteAllText(seedFlagPath, $"Seeded at {DateTime.UtcNow}");
                    Console.WriteLine(">>>> DENSE SEEDING COMPLETE <<<<");
                }
            }

            // ── Recurring Jobs (Delayed to prevent lock timeout during startup) ──
            Task.Run(async () => {
                await Task.Delay(2000);
                try {
                    RecurringJob.AddOrUpdate<ISeatService>(
                        "release-expired-locks",
                        service => service.ReleaseExpiredLocksAsync(),
                        "*/30 * * * * *");

                    RecurringJob.AddOrUpdate<IScheduleService>(
                        "process-daily-schedules",
                        service => service.ProcessSchedulesJobAsync(),
                        Cron.Daily);
                } catch { /* Handle lock timeout gracefully */ }
            });

            await app.RunAsync();
        }
    }
}
