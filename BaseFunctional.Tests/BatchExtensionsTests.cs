namespace BaseFunctional.Tests;

public class BatchExtensionsArrayTests
{
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(20)]
    public void Batch_Array_BatchSizes(int batchSize)
    {
        var array = Enumerable.Range(1, 42).ToArray();
        var batches = array.Batch(batchSize).ToList();

        // Check batch count
        var expectedBatchCount = (int)Math.Ceiling(array.Length / (double)batchSize);
        batches.Count.Should().Be(expectedBatchCount);

        // All but last batch should have batchSize elements
        for (var i = 0; i < batches.Count - 1; i++)
        {
            batches[i].Should().HaveCount(batchSize);
            batches[i].Should().BeEquivalentTo(array.Skip(i * batchSize).Take(batchSize));
        }

        // Last batch may be smaller
        var lastBatch = batches.Last();
        var expectedLastBatchSize = array.Length % batchSize == 0 ? batchSize : array.Length % batchSize;
        lastBatch.Should().HaveCount(expectedLastBatchSize);
        lastBatch.Should().BeEquivalentTo(array.Skip((batches.Count - 1) * batchSize).Take(expectedLastBatchSize));
    }
}

public class BatchExtensionsListTests
{
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(20)]
    public void Batch_List_BatchSizes(int batchSize)
    {
        var list = Enumerable.Range(1, 42).ToList();
        var batches = list.Batch(batchSize).ToList();

        // Check batch count
        var expectedBatchCount = (int)Math.Ceiling(list.Count / (double)batchSize);
        batches.Count.Should().Be(expectedBatchCount);

        // All but last batch should have batchSize elements
        for (var i = 0; i < batches.Count - 1; i++)
        {
            batches[i].Should().HaveCount(batchSize);
            batches[i].Should().BeEquivalentTo(list.Skip(i * batchSize).Take(batchSize));
        }

        // Last batch may be smaller
        var lastBatch = batches.Last();
        var expectedLastBatchSize = list.Count % batchSize == 0 ? batchSize : list.Count % batchSize;
        lastBatch.Should().HaveCount(expectedLastBatchSize);
        lastBatch.Should().BeEquivalentTo(list.Skip((batches.Count - 1) * batchSize).Take(expectedLastBatchSize));
    }
}