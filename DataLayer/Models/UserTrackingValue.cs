using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserTrackingValue
    {
        [Key]
        public int UserTrackingValueId { get; init; }
        public int UserTrackingId { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public UserTrackingType Type { get; init; } = UserTrackingType.Number;
        public int Order { get; init; }
        public bool Disabled { get; init; }
        public string Metadata { get; init; }
        public virtual UserTracking Tracking { get; init; }
        public virtual ICollection<UserDailyTrackingValue> DailyTrackingValues { get; init; }
    }

    public enum UserTrackingType
    {
        Number,
        WholeNumber,
        Boolean,
        Icon,
    }
}