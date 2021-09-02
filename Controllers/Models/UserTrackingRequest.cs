using System.Collections.Generic;

namespace diet_tracker_api.Controllers.Models
{
    public record UserTrackingRequest(string Title, string Description, int Occurrences, int Order, bool Disabled, bool UseTime, IEnumerable<UserTrackingValueRequest> Values);
}
