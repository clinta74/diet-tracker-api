using System.ComponentModel.DataAnnotations.Schema;

namespace diet_tracker_api.DataLayer.Models;

public record UserDailyTrackingValue
{
    public string UserId { get; init; } = null!;
    [Column(TypeName = "date")]
    public DateTime Day { get; init; }
    public int Occurrence { get; init; }
    public int UserTrackingValueId { get; init; }
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Value { get; init; }
    // A time of day as entered (the UI sends 0001-01-01T08:30), like the other When columns.
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? When { get; init; }
    public virtual UserDay? UserDay { get; init; }
    public virtual UserTrackingValue? TrackingValue { get; init; }
}