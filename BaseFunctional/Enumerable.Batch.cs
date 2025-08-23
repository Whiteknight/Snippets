using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class BatchExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        NotZeroOrNegative(batchSize);
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
