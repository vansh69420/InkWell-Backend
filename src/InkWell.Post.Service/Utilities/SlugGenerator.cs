using System.Text.RegularExpressions;

namespace InkWell.Post.Service.Utilities;

public static class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var text = input.Trim().ToLowerInvariant();
        text = Regex.Replace(text, @"[^a-z0-9]+", "-");
        text = text.Trim('-');
        text = Regex.Replace(text, @"-+", "-");
        return text;
    }
}