using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.Models
{
    public class OperatorProfile
    {
        [Key]
        public Guid ProfileId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required, MaxLength(300)]
        public string BusinessName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? LicenseDocPath { get; set; }

        [MaxLength(500)]
        public string ContactDetails { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
    }
}
