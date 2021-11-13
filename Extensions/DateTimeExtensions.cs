using System;

namespace diet_tracker_api.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateOnly ToDateOnly(this DateTime dateTime) => new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }
}