using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserMeal
    {
        [Key]
        public int UserMealId { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Day { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Calories { get; set; }
        public virtual UserDay UserDay { get; set; }     
    }
}