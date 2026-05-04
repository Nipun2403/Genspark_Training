using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniBus.Server.Models
{
    public class BusSchedule
    {
        [Key]
        public Guid ScheduleId { get; set; } = Guid.NewGuid();

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

        [MaxLength(500)]
        public string PickupAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DropoffAddress { get; set; } = string.Empty;

        public TimeSpan DepartureTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
