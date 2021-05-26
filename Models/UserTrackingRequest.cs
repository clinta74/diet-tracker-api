using System.Collections.Generic;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.Models
{
    public record UserTrackingRequest(string Title, string Description, int Occurrences, int Order, IEnumerable<UserTrackingValueRequest> Values);
    public record UserTrackingValueRequest(string Name, string Description, UserTrackingType Type, int Order);
}