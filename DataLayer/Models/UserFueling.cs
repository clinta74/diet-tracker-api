using System;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserFueling
    {
        [Key]
        public int UserFuelingId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime When { get; set; }
        public virtual User User { get; set; }
    }
}