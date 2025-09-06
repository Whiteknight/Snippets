using static BaseFunctional.Assert;

namespace DataTypeRelated;

public static class ExceptionEnumerateExtensions
{
    public static IEnumerable<Exception> Enumerate(this Exception exception)
    {
        var worklist = new Queue<Exception>();
        worklist.Enqueue(NotNull(exception));

        static void AddAggregateInners(AggregateException agg, Queue<Exception> list)
        {
            foreach (var inner in agg.InnerExceptions)
                list.Enqueue(inner);
        }

        while (worklist.Count != 0)
        {
            var ex = worklist.Dequeue();
            if (ex is AggregateException agg)
                AddAggregateInners(agg, worklist);
            else if (ex.InnerException is not null)
                worklist.Enqueue(ex.InnerException);
            yield return ex;
        }
    }
}
