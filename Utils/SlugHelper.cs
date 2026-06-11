using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MenuOnline.Utils;

public static class SlugHelper
{
  public static string Generate(string name)
  {
    if (string.IsNullOrWhiteSpace(name)) return string.Empty;

    var normalized = name.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();
    foreach (var c in normalized)
    {
      var category = CharUnicodeInfo.GetUnicodeCategory(c);
      if (category == UnicodeCategory.NonSpacingMark) continue;
      sb.Append(c);
    }

    var slug = sb.ToString().ToLowerInvariant();
    slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
    slug = slug.Trim('-');

    return slug;
  }
}
