using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserTracking
    {
        [Key]
        public int UserTrackingId { get; init; }
        public string UserId { get; init; }
        public bool Disabled { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public int Occurrences { get; init; }
        public int Order { get; init; }
        public bool UseTime { get; init; }
        public virtual User User { get; init; }
        public virtual IEnumerable<UserTrackingValue> Values { get; init; }
    }
}