using Microsoft.EntityFrameworkCore;
using OmniBus.Server.Models;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Data
{
    public class OmniBusDbContext : DbContext
    {
        public OmniBusDbContext(DbContextOptions<OmniBusDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Models.Route> Routes => Set<Models.Route>();
        public DbSet<Bus> Buses => Set<Bus>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<SeatLock> SeatLocks => Set<SeatLock>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();
        public DbSet<OperatorProfile> OperatorProfiles => Set<OperatorProfile>();
        public DbSet<BusSchedule> BusSchedules => Set<BusSchedule>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── pg_trgm indexes for fuzzy search ──
            mb.HasPostgresExtension("pg_trgm");

            // ── User ──
            mb.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            });

            // ── Route ──
            mb.Entity<Models.Route>(e =>
            {
                e.HasIndex(r => new { r.SourceCity, r.DestinationCity }).IsUnique();
                // GIN trigram indexes for fuzzy search
                e.HasIndex(r => r.SourceCity)
                    .HasMethod("gin")
                    .HasOperators("gin_trgm_ops");
                e.HasIndex(r => r.DestinationCity)
                    .HasMethod("gin")
                    .HasOperators("gin_trgm_ops");
            });

            // ── Bus ──
            mb.Entity<Bus>(e =>
            {
                e.HasIndex(b => new { b.PlateNumber, b.RouteId }).IsUnique();
                e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
                e.HasOne(b => b.Operator).WithMany().HasForeignKey(b => b.OperatorId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(b => b.Route).WithMany(r => r.Buses).HasForeignKey(b => b.RouteId).OnDelete(DeleteBehavior.Restrict);
            });

            // ── Seat ──
            mb.Entity<Seat>(e =>
            {
                e.HasIndex(s => new { s.BusId, s.SeatNumber }).IsUnique();
                e.HasOne(s => s.Bus).WithMany(b => b.Seats).HasForeignKey(s => s.BusId).OnDelete(DeleteBehavior.Cascade);
            });

            // ── SeatLock ──
            mb.Entity<SeatLock>(e =>
            {
                e.HasIndex(sl => new { sl.SeatId, sl.ExpiresAt });
                e.HasOne(sl => sl.Seat).WithMany(s => s.SeatLocks).HasForeignKey(sl => sl.SeatId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(sl => sl.User).WithMany(u => u.SeatLocks).HasForeignKey(sl => sl.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // ── Booking ──
            mb.Entity<Booking>(e =>
            {
                e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
                e.HasOne(b => b.User).WithMany(u => u.Bookings).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(b => b.Bus).WithMany(b => b.Bookings).HasForeignKey(b => b.BusId).OnDelete(DeleteBehavior.Restrict);
            });

            // ── BookingSeat ──
            mb.Entity<BookingSeat>(e =>
            {
                e.HasIndex(bs => new { bs.BookingId, bs.SeatId }).IsUnique();
                e.Property(bs => bs.PassengerGender).HasConversion<string>().HasMaxLength(10);
                e.HasOne(bs => bs.Booking).WithMany(b => b.BookingSeats).HasForeignKey(bs => bs.BookingId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(bs => bs.Seat).WithMany(s => s.BookingSeats).HasForeignKey(bs => bs.SeatId).OnDelete(DeleteBehavior.Restrict);
            });

            // ── Coupon ──
            mb.Entity<Coupon>(e =>
            {
                e.HasIndex(c => c.Code).IsUnique();
            });

            // ── OtpRecord ──
            mb.Entity<OtpRecord>(e =>
            {
                e.HasIndex(o => new { o.Email, o.ExpiresAt });
            });

            // ── OperatorProfile ──
            mb.Entity<OperatorProfile>(e =>
            {
                e.HasIndex(op => op.UserId).IsUnique();
                e.Property(op => op.ApprovalStatus).HasConversion<string>().HasMaxLength(20);
                e.HasOne(op => op.User).WithOne(u => u.OperatorProfile).HasForeignKey<OperatorProfile>(op => op.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // ── BusSchedule ──
            mb.Entity<BusSchedule>(e =>
            {
                e.HasOne(s => s.Operator).WithMany().HasForeignKey(s => s.OperatorId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(s => s.Route).WithMany().HasForeignKey(s => s.RouteId).OnDelete(DeleteBehavior.Restrict);
            });

            // ── Seed Admin ──
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            mb.Entity<User>().HasData(new User
            {
                UserId = adminId,
                Email = "admin@omnibus.com",
                FullName = "System Admin",
                Phone = "0000000000",
                Role = UserRole.Admin,
                IsVerified = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
