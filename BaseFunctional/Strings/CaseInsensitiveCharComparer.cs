using System.Diagnostics.CodeAnalysis;

namespace BaseFunctional.Strings;

public sealed class CaseInsensitiveCharComparer : IEqualityComparer<char>
{
    public static IEqualityComparer<char> Instance { get; } = new CaseInsensitiveCharComparer();

    public bool Equals(char x, char y) => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);

    public int GetHashCode([DisallowNull] char obj) => char.ToUpperInvariant(obj).GetHashCode();
}
