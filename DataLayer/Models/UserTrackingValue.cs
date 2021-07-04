using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserTrackingValue
    {
        [Key]
        public int UserTrackingValueId { get; set; }
        public int UserTrackingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UserTrackingType Type { get; set; }
        public int Order { get; set; }
        public bool Disabled { get; set; }
        public virtual UserTracking Tracking { get; set; }
        public virtual ICollection<UserDailyTrackingValue> DailyTrackingValues { get; set; }
    }

    public enum UserTrackingType
    {
        Number,
        WholeNumber,
        Boolean,
    }
}