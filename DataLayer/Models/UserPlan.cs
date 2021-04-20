using System;

namespace diet_tracker_api.DataLayer.Models
{
    public class UserPlan
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime Start { get; set; }
        public virtual Plan Plan { get; set; }
        public virtual User User { get; set; }
    }
}