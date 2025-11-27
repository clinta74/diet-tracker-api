using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record Victory
    {
        [Key]
        public int VictoryId { get; init; }
        public string UserId { get; init; }
        public string Name { get; init; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? When { get; init; }
        public VictoryType Type { get; init; }
        public virtual User User { get; init; }
    }

    public enum VictoryType
    {
        NonScale,
        Goal,
    }
}