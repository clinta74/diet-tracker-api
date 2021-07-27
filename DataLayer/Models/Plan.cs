using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace diet_tracker_api.DataLayer.Models
{
    public record Plan
    {
        [Key]
        public int PlanId { get; init; }
        public string Name { get; init; }
        public int FuelingCount { get; init; }
        public int MealCount { get; init; }
        public virtual ICollection<UserPlan> UserPlans { get; init; }
    }
}