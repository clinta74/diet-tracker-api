using System;

namespace diet_tracker_api.Models
{
public record CurrentUserDailyTracking
    {
        public string UserId { get; set; }
        public DateTime Day { get; set; }
        public int Value { get; set; }
        public DateTime When { get; set; }
        public int Occurance { get; set; }
        public int UserTrackingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}