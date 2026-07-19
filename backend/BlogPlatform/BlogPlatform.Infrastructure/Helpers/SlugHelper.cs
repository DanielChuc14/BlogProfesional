using System.Text;
using System.Text.RegularExpressions;

namespace BlogPlatform.Infrastructure.Helpers;

public static partial class SlugHelper
{
    public static string Generate(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);

        var withoutDiacritics = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                withoutDiacritics.Append(c);
        }

        var slug = withoutDiacritics.ToString().ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');

        return slug.Length > 300 ? slug[..300] : slug;
    }

    public static string MakeUnique(string slug, IEnumerable<string> existingSlugs)
    {
        if (!existingSlugs.Contains(slug))
            return slug;

        var counter = 1;
        string candidate;
        do
        {
            candidate = $"{slug}-{counter++}";
        } while (existingSlugs.Contains(candidate));

        return candidate;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex MultipleHyphensRegex();
}
