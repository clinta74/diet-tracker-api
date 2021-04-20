using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserDay
    {
        public string UserId { get; set; }
        public DateTime Day { get; set; }
        public int Water { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Weight { get; set; }
        public int Condiments { get; set; }

        public virtual User User { get; set; }        
    }
}