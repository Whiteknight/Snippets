namespace DataTypeRelated;

public static class DateTimeRangesExtensions
{
    public static bool IsWithinDaysOf(this DateTime thisDate, DateTime now, int daysMin, int daysMax)
    {
        var daysDiff = (int)Math.Round((now - thisDate).TotalDays, MidpointRounding.AwayFromZero);
        return daysDiff.IsBetweenInclusive(daysMin, daysMax);
    }

    public static bool IsWithinDaysOfNow(this DateTime thisDate, int daysMin, int daysMax)
        => thisDate.IsWithinDaysOf(DateTime.UtcNow, daysMin, daysMax);
}
