using System;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GraphValue(decimal value, DateOnly date);
    public record GetGraphData(string UserId, DateOnly StartDate, Nullable<DateOnly> EndDate);
}