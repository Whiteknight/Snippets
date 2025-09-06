namespace DataTypeRelated;

public static class IntBetweenExtensions
{
    public static bool IsBetweenInclusive(this int thisInt, int min, int max)
        => min <= max
            ? min <= thisInt && thisInt <= max
            : throw new ArgumentException("Minimum is larger than maximum. Comparison is invalid.");
}
