using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniBus.Server.Models
{
    public class SeatLock
    {
        [Key]
        public Guid LockId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SeatId { get; set; }

        [ForeignKey(nameof(SeatId))]
        public Seat Seat { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
