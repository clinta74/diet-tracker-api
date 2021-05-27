using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserDailyTrackingValue
    {
        public string UserId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Day { get; set; }
        public int Occurrence { get; set; }
        public int UserTrackingValueId { get; set; }
        public int Value { get; set; }
        public DateTime When { get; set; }
        public virtual UserDay UserDay { get; set; }
        public virtual UserTrackingValue TrackingValue { get; set; }
    }
}