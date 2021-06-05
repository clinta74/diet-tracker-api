using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserFueling
    {
        [Key]
        public int UserFuelingId { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Day { get; set; }

        public string Name { get; set; }
        public Nullable<DateTime> When { get; set; }
        public virtual UserDay UserDay { get; set; }
    }
}