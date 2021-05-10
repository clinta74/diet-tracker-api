using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record User
    {
        [Key]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Created { get; set; }
        public int WaterTarget { get; set; }
        public int WaterSize { get; set; }
        public virtual ICollection<UserDay> UserDays { get; set; }
        public virtual ICollection<UserFueling> UserFuelings { get; set; }
        public virtual ICollection<UserMeal> UserMeals { get; set; }
        public virtual ICollection<UserPlan> UserPlans { get; set; }
        public virtual ICollection<UserTracking> UserTrackings { get; set; }
        public virtual ICollection<Victory> Victories { get; set; }
    }
}