using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record Victory
    {
        [Key]
        public int VictoryId { get; init; }
        public string UserId { get; init; }
        public string Name { get; init; }
        public DateTime? When { get; init; }
        public VictoryType Type { get; init; }
        public User User { get; init; }
    }

    public enum VictoryType
    {
        NonScale,
        Goal,
    }
}