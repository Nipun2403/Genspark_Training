using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Models
{
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public Guid BusId { get; set; }

        [ForeignKey(nameof(BusId))]
        public Bus Bus { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseFare { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Gst { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ServiceFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BookingFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AdminCommission { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal RefundAmount { get; set; } = 0;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [MaxLength(100)]
        public string? PaymentRef { get; set; }

        [MaxLength(50)]
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; } = 0;

        [MaxLength(1000)]
        public string? CancellationNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
