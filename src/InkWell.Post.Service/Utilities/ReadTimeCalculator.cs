using System.Text.RegularExpressions;

namespace InkWell.Post.Service.Utilities;

public static class ReadTimeCalculator
{
    public static int ComputeMinutesFromHtml(string html, int wpm = 200)
    {
        if (string.IsNullOrWhiteSpace(html)) return 1;

        // strip tags
        var text = Regex.Replace(html, "<.*?>", " ");
        text = Regex.Replace(text, @"\s+", " ").Trim();

        if (string.IsNullOrWhiteSpace(text)) return 1;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var minutes = (int)Math.Ceiling(words / (double)wpm);
        return Math.Max(1, minutes);
    }

    public static string BuildExcerptFromHtml(string html, int maxLen = 200)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var text = Regex.Replace(html, "<.*?>", " ");
        text = Regex.Replace(text, @"\s+", " ").Trim();

        if (text.Length <= maxLen) return text;
        return text[..maxLen].Trim() + "...";
    }
}