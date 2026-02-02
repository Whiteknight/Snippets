using BaseFunctional.Calculations;
using BaseFunctional.Strings;

namespace BaseFunctional;

public static class StringDistanceExtensions
{
    public static int DistanceFrom(this string source, string target, bool caseSensitive = true)
        => LevenshteinDistance.CalculateStringDistance(source, target, caseSensitive ? EqualityComparer<char>.Default : CaseInsensitiveCharComparer.Instance);
}
