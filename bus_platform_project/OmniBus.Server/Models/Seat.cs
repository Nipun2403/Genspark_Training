using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniBus.Server.Models
{
    public class Seat
    {
        [Key]
        public Guid SeatId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusId { get; set; }

        [ForeignKey(nameof(BusId))]
        public Bus Bus { get; set; } = null!;

        [Range(1, 42)]
        public int SeatNumber { get; set; }

        // Navigation
        public ICollection<SeatLock> SeatLocks { get; set; } = new List<SeatLock>();
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
