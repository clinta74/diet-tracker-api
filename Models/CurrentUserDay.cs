using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.Models
{
    public record CurrentUserDay : UserDay
    {
        public decimal CumulativeWeightChange { get; set; }
        public decimal WeightChange { get; set; }
    }
}