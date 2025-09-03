namespace BaseFunctional;

public static class StripQuotesExtensions
{
    public static string StripQuotes(this string? value, bool handleBackslashEscapes = false)
    {
        if (string.IsNullOrEmpty(value) || value.Length == 1)
            return value ?? string.Empty;

        if (!handleBackslashEscapes)
        {
            if (value[0] == '"' && value[^1] == '"')
                return value[1..^1];
            if (value[0] == '\'' && value[^1] == '\'')
                return value[1..^1];
            return value;
        }

        char[] buffer = new char[value.Length];
        int ptr = 0;
        int i = 0;
        for (; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '"' || c == '\'')
                continue;
            if (c == '\\' && i < value.Length - 1)
            {
                i++;
                buffer[ptr++] = value[i];
                continue;
            }

            buffer[ptr++] = c;
        }

        if (i <= value.Length - 1)
        {
            var c = value[i];
            if (c != '"' && c != '\'')
                buffer[ptr++] = value[i];
        }

        return new string(buffer, 0, ptr);
    }
}
