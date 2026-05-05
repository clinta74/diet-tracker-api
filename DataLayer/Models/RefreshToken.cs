using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record RefreshToken
    {
        [Key]
        public int Id { get; init; }

        [Required]
        [MaxLength(250)]
        public string UserId { get; init; }

        [Required]
        public string TokenHash { get; init; }

        public DateTime ExpiresAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? RevokedAt { get; init; }
        public int? ReplacedByTokenId { get; init; }

        [MaxLength(45)]
        public string CreatedByIp { get; init; }

        public virtual User User { get; init; }
    }
}
