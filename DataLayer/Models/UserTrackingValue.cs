using System;
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
        public UserTrackingType Type { get; set; } = UserTrackingType.Number;
        public int Order { get; set; }
        public int Min { get; set; } = 0;
        public Nullable<int> Max { get; set; } = null;
        public bool Disabled { get; set; }
        public virtual UserTracking Tracking { get; set; }
        public virtual ICollection<UserDailyTrackingValue> DailyTrackingValues { get; set; }
    }

    public enum UserTrackingType
    {
        Number,
        WholeNumber,
        Boolean,
        Icon,
    }
}