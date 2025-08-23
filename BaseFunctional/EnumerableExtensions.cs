using System.Diagnostics.CodeAnalysis;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class EnumerableExtensions
{
    [return: NotNull]
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];

    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        NotNull(action);
        foreach (var item in source.OrEmptyIfNull())
        {
            action(item);
            yield return item;
        }
    }

    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);
        int count = 0;
        foreach (var item in source.OrEmptyIfNull())
        {
            batch.Add(item);
            count++;
            if (count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }
}
