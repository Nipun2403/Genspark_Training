using System.ComponentModel.DataAnnotations;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public OperatorProfile? OperatorProfile { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
    }
}
