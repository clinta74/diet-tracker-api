using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserPermission
    {
        [MaxLength(250)]
        public string UserId { get; init; }

        [MaxLength(100)]
        public string Permission { get; init; }

        public virtual User User { get; init; }
    }
}
