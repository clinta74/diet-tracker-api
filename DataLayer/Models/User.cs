using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record User
    {
        [Key]
        [MaxLength(250)]
        public string UserId { get; init; }

        [Required]
        public string FirstName { get; init; }

        [Required]
        public string LastName { get; init; }
        
        [EmailAddress]
        public string EmailAddress { get; init; }
        public DateTime Created { get; init; }
        public int WaterTarget { get; init; }
        public int WaterSize { get; init; }
        public bool Autosave { get; init; }
        public virtual ICollection<UserDay> UserDays { get; init; }
        public virtual ICollection<UserFueling> UserFuelings { get; init; }
        public virtual ICollection<UserMeal> UserMeals { get; init; }
        public virtual ICollection<UserPlan> UserPlans { get; init; }
        public virtual ICollection<UserTracking> UserTrackings { get; init; }
        public virtual ICollection<Victory> Victories { get; init; }
    }
}