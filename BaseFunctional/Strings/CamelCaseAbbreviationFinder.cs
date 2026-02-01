using System.Diagnostics;
using static BaseFunctional.Assert;

namespace BaseFunctional.Strings;

public static class CamelCaseAbbreviationFinder
{
    public static string AbbreviateCamelCase(this string camelCase, int length, char padChar = '\0')
    {
        var abbreviation = FindAbbreviation(camelCase, length);
        Debug.Assert(abbreviation.Length <= length);
        if (padChar == '\0')
            return abbreviation;

        return abbreviation.Length == length
            ? abbreviation
            : abbreviation + new string(padChar, length - abbreviation.Length);
    }

    public static string FindAbbreviation(string camelCase, int length)
    {
        NotZeroOrNegative(length);
        if (string.IsNullOrEmpty(camelCase))
            return string.Empty;

        Span<(int Position, char C)> buffer = stackalloc (int, char)[length];
        buffer[0] = (0, char.ToUpper(camelCase[0]));
        int j = 1;

        // Look for upper-case characters or numbers until we have enough.
        for (int i = 1; i < camelCase.Length && j < length; i++)
        {
            var c = camelCase[i];
            if (char.IsUpper(c) || char.IsNumber(c))
                buffer[j++] = (i, c);
        }

        if (j >= length)
            return CreateResult(buffer, length);

        // Go back and get enough lower-case letters to fill
        for (int i = 1; i < camelCase.Length && j < length; i++)
        {
            var c = camelCase[i];
            if (char.IsLower(c))
                buffer[j++] = (i, c);
        }

        return CreateResult(buffer, j < length ? j : length);
    }

    private static string CreateResult(Span<(int Position, char C)> buffer, int size)
    {
        buffer.Sort(static (x, y) => x.Position.CompareTo(y.Position));
        Span<char> result = stackalloc char[size];
        for (int i = 0; i < size; i++)
            result[i] = buffer[i].C;
        return new string(result);
    }
}
