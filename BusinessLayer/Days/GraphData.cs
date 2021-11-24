using System;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GraphValue(decimal value, DateTime date);
    public record GetGraphData(string UserId, DateTime StartDate, Nullable<DateTime> EndDate);
}