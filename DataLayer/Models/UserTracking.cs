using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserTracking
    {
        [Key]
        public int UserTrackingId { get; set; }
        public string UserId { get; set; }
        public bool Removed { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Occurrences { get; set; }
        public UserTrackingType Type { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<UserDailyTracking> Trackings { get; set; }
    }

    public enum UserTrackingType
    {
        Number,
        Boolean,
    }
}