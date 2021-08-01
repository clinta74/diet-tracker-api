namespace diet_tracker_api.DataLayer.Models
{
    public record UserTrackingValueMetadata
    {
        public int UserTrackingValueId { get; init; }
        public string Key { get; init; }
        public string Value { get; init; }
        public virtual UserTrackingValue UserTrackingValue { get; init; }
    }
}