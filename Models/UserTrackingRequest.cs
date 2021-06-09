using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.Models
{
    public record UserTrackingRequest(string Title, string Description, int Occurrences, int Order, bool Disabled);
    public record UserTrackingValueRequest(int UserTrackingId, string Name, string Description, UserTrackingType Type, int Order, bool Disabled);
}