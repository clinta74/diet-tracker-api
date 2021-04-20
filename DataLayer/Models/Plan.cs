using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace diet_tracker_api.DataLayer.Models
{
    public record Plan
    {
        [Key]
        public int PlanId { get; set; }
        public string Name { get; set; }
        public int FuelingCount { get; set; }
        public int MealCount { get; set; }
        public virtual ICollection<UserPlan> UserPlans { get; set; }
    }
}