using System.Text.RegularExpressions;

namespace SchoolCRM.Shared.Helpers;

public static class StringHelper
{
    public static string GenerateAdmissionNumber(string prefix = "ADM", int sequence = 1)
    {
        return $"{prefix}-{DateTime.Now:yyyy}-{sequence:D5}";
    }

    public static string GenerateEmployeeCode(string prefix = "EMP", int sequence = 1)
    {
        return $"{prefix}-{DateTime.Now:yyyy}-{sequence:D5}";
    }

    public static string GenerateReceiptNumber(string prefix = "RCP", int sequence = 1)
    {
        return $"{prefix}-{DateTime.Now:yyyyMMdd}-{sequence:D5}";
    }

    public static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        text = text.ToLower().Trim();
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
        text = Regex.Replace(text, @"[\s-]+", "-");
        return text;
    }

    public static string Truncate(this string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength) return text;
        return text[..maxLength] + "...";
    }

    public static string StripHtml(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return Regex.Replace(text, "<.*?>", string.Empty).Trim();
    }

    public static string ToTitleCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.TextInfo.ToTitleCase(text.ToLower());
    }
}
