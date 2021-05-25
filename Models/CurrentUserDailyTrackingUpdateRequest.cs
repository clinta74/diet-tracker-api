using System;
using System.Collections.Generic;

namespace diet_tracker_api.Models
{
    public record CurrentUserDailyTrackingValueRequest
    {
        public int Value { get; set; }
        public DateTime When { get; set; }
    }
}