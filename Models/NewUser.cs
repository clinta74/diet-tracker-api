using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diet_tracker_api.Models
{
    public record NewUser
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public int PlanId { get; set; }
    }
}