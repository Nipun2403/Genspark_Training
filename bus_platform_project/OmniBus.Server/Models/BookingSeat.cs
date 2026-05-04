using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Models
{
    public class BookingSeat
    {
        [Key]
        public Guid BookingSeatId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookingId { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

        [Required]
        public Guid SeatId { get; set; }

        [ForeignKey(nameof(SeatId))]
        public Seat Seat { get; set; } = null!;

        [Required, MaxLength(200)]
        public string PassengerName { get; set; } = string.Empty;

        [Range(1, 120)]
        public int PassengerAge { get; set; }

        public Gender PassengerGender { get; set; }

        [Required, MaxLength(20)]
        public string PassengerMobile { get; set; } = string.Empty;
    }
}
