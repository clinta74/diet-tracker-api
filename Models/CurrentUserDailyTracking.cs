using System;
using System.Collections.Generic;

namespace diet_tracker_api.Models
{
    public record CurrentUserDailyTrackingValue
    {
        public DateTime Day { get; set; }
        public string UserId { get; set; }
        public int UserTrackingValueId { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public Nullable<DateTime> When { get; set; }
        public int Order { get; set; }
        public int Occurrence { get; set; }
        public string Description { get; set; }
    }
}