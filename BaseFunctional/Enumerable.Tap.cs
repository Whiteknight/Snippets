using static BaseFunctional.Assert;

namespace BaseFunctional;
public static class TapExtensions
{
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
