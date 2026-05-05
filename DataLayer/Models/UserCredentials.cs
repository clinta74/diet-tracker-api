using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserCredentials
    {
        [Key]
        [MaxLength(250)]
        public string UserId { get; init; }

        [Required]
        [MaxLength(254)]
        public string Email { get; init; }

        [Required]
        public string PasswordHash { get; init; }

        public virtual User User { get; init; }
    }
}
