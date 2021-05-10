using System;
using System.Collections.Generic;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.Models
{
    public record CurrentUser
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Created { get; set; }
        public Plan CurrentPlan { get; set; }
        public DateTime? Started { get; set; }
    }
}