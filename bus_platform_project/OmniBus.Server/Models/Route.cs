using System.ComponentModel.DataAnnotations;

namespace OmniBus.Server.Models
{
    public class Route
    {
        [Key]
        public Guid RouteId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string SourceCity { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string DestinationCity { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    }
}
