using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class TapExtensions
{
    // Execute a callback on each item of an enumerable, as it's being enumerated.
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        NotNull(action);
        foreach (var item in source.OrEmptyIfNull())
        {
            action(item);
            yield return item;
        }
    }
}
