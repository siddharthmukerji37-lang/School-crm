namespace SchoolCRM.Shared.Helpers;

public static class NumberHelper
{
    public static string ToCurrency(this decimal amount, string currencySymbol = "$")
    {
        return $"{currencySymbol}{amount:N2}";
    }

    public static string ToPercentage(this decimal value, int decimals = 1)
    {
        return $"{value.ToString($"F{decimals}")}%";
    }

    public static decimal CalculatePercentage(this decimal value, decimal total)
    {
        if (total == 0) return 0;
        return Math.Round((value / total) * 100, 2);
    }

    public static string ToFileSize(this long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
