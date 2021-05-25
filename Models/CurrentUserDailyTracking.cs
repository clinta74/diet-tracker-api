using System;
using System.Collections.Generic;

namespace diet_tracker_api.Models
{
    public record CurrentUserDailyTracking
    {
        public string UserId { get; set; }
        public DateTime Day { get; set; }
        public int Occurrence { get; set; }
        public int UserTrackingId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public IEnumerable<CurrentUserDailyTrackingValue> Values { get; set; }
    }

    public record CurrentUserDailyTrackingValue
    {
        public int UserTrackingValueId { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public DateTime When { get; set; }
        public int Order { get; set; }
    }
}