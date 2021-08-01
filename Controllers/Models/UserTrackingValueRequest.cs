using System.Collections.Generic;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.Controllers.Models
{
    public record UserTrackingValueRequest
    {
        public int UserTrackingValueId { get; init; }
        public int UserTrackingId { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public UserTrackingType Type { get; init; }
        public int Order { get; init; }
        public bool Disabled { get; init; }
        public IEnumerable<UserTrackingValueMetadata> Metadata { get; init; }
    }
}
