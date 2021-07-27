using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record Preference
    {
        [Key]
        public string UserId { get; init; }
    }
}