using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserDay
    {
        public string UserId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Day { get; set; }
        public int Water { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Weight { get; set; }
        public int Condiments { get; set; }
        public string Notes { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<UserFueling> Fuelings { get; set; }
        public virtual ICollection<UserMeal> Meals { get; set; }
        public virtual ICollection<UserDailyTracking> Trackings { get; set; }
    }
}