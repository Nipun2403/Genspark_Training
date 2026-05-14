using Microsoft.EntityFrameworkCore;
using SharedModels;

namespace DataAccess
{
  public class AppDbContext : DbContext
  {
    public AppDbContext() { }

    public DbSet<User> Users { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    // Hardcode the connection string and not not using appsettings.json
    // EF Core will use this for BOTH the CLI migrations and the running console.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=peewee;Password=");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<NotificationLog>()
          .HasKey(n => n.LogId);

      modelBuilder.Entity<NotificationLog>()
          .HasOne(n => n.User)
          .WithMany(u => u.NotificationLogs)
          .HasForeignKey(n => n.UserId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}