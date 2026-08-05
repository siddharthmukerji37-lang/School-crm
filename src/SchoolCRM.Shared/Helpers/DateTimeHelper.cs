namespace SchoolCRM.Shared.Helpers;

public static class DateTimeHelper
{
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hour(s) ago";
        if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} day(s) ago";
        if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
        return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
    }

    public static int CalculateAge(this DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    public static DateTime GetStartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }

    public static DateTime GetStartOfMonth(this DateTime date) => new(date.Year, date.Month, 1);
    public static DateTime GetEndOfMonth(this DateTime date) => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    public static DateTime GetStartOfYear(this DateTime date) => new(date.Year, 1, 1);
    public static DateTime GetEndOfYear(this DateTime date) => new(date.Year, 12, 31);
}
