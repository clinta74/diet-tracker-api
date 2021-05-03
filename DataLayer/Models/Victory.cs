using System;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record Victory
    {
        [Key]
        public int VictoryId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public Nullable<DateTime> When { get; set; }
        public VictoryType Type { get; set; }
        public User User { get; set; }
    }

    public enum VictoryType
    {
        NonScale,
        Goal,
    }
}