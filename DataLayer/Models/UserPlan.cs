namespace diet_tracker_api.DataLayer.Models
{
    public record UserPlan
    {
        public string UserId { get; init; }
        public int PlanId { get; init; }
        public DateTime Start { get; init; }
        public virtual Plan Plan { get; init; }
        public virtual User User { get; init; }
    }
}