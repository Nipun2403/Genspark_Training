using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Models
{
    public class Bus
    {
        [Key]
        public Guid BusId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OperatorId { get; set; }

        [ForeignKey(nameof(OperatorId))]
        public User Operator { get; set; } = null!;

        [Required]
        public Guid RouteId { get; set; }

        [ForeignKey(nameof(RouteId))]
        public Route Route { get; set; } = null!;

        [Required, MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string BusNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        public int TotalSeats { get; set; } = 42;

        public BusStatus Status { get; set; } = BusStatus.PendingApproval;

        [MaxLength(500)]
        public string PickupAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DropoffAddress { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
