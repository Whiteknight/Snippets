using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class BatchExtensions
{
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        static IEnumerable<IReadOnlyList<T>> IterateNonList(IEnumerable<T> source, int batchSize)
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
                    count = 0;
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        static IEnumerable<IReadOnlyList<T>> IterateList(List<T> source, int batchSize)
        {
            for (int i = 0; i < source.Count; i += batchSize)
                yield return source.Slice(i, Math.Min(source.Count - i, batchSize));
        }

        static IEnumerable<IReadOnlyList<T>> IterateArray(T[] source, int batchSize)
        {
            for (int i = 0; i < source.Length; i += batchSize)
                yield return new ArraySegment<T>(source, i, Math.Min(source.Length - i, batchSize)).ToArray();
        }

        NotZeroOrNegative(batchSize);
        return NotNull(source) switch
        {
            List<T> list => IterateList(list, batchSize),
            T[] array => IterateArray(array, batchSize),
            _ => IterateNonList(source, batchSize)
        };
    }
}
