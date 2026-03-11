namespace DailyContentWriter.Utils;

public static class TimeScheduleHelper
{
    public static bool IsScheduledTime(DateTime now, IEnumerable<string> scheduleTimes)
    {
        var current = now.ToString("HH:mm");
        return scheduleTimes.Any(t => string.Equals(t, current, StringComparison.OrdinalIgnoreCase));
    }

    public static string BuildExecutionKey(DateTime now)
    {
        return now.ToString("yyyy-MM-dd HH:mm");
    }
}