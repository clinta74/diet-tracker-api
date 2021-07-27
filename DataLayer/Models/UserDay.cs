using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserDay
    {
        public string UserId { get; init; }

        [Column(TypeName = "date")]
        public DateTime Day { get; init; }
        public int Water { get; init; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Weight { get; init; }
        public string Notes { get; init; }
        public virtual User User { get; init; }
        public virtual ICollection<UserFueling> Fuelings { get; init; }
        public virtual ICollection<UserMeal> Meals { get; init; }
        public virtual ICollection<UserDailyTrackingValue> TrackingValues { get; init; }
    }
}