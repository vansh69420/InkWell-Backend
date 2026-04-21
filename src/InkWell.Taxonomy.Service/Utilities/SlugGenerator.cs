using System.Text;
using System.Text.RegularExpressions;

namespace InkWell.Taxonomy.Service.Utilities;

public static class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var text = input.Trim().ToLowerInvariant();

        // Replace non-alphanumeric with hyphens
        text = Regex.Replace(text, @"[^a-z0-9]+", "-");

        // Trim hyphens
        text = text.Trim('-');

        // Collapse multiple hyphens
        text = Regex.Replace(text, @"-+", "-");

        return text;
    }
}