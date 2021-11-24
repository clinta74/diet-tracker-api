using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models
{
    public record UserMeal
    {
        [Key]
        public int UserMealId { get; init; }
        public string UserId { get; init; }
        [Column(TypeName = "date")]
        public DateTime Day { get; init; }
        public string Name { get; init; }
        public Nullable<DateTime> When { get; init; }
        public virtual UserDay UserDay { get; init; }     
    }
}