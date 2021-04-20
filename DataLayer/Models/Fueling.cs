using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record Fueling
    {
        [Key]
        public int FuelingId { get; set; }
        public string Name { get; set; }
    }
}