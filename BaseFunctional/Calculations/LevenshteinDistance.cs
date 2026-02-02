using System.Buffers;

namespace BaseFunctional.Calculations;

public static class LevenshteinDistance
{
    public static int CalculateStringDistance(string s, string t, IEqualityComparer<char> compare)
    {
        // The single-letter variable names are from the literature and are kept for consistency.
        // s is the source string with a length of m
        // t is the target string with a length of n
        // v is the workspace matrix, which in this case for an optimization is just two rows that we swap
        if (s.Length == 0)
            return t.Length;
        if (t.Length == 0)
            return s.Length;
        int n = t.Length;
        int m = s.Length;
        int totalSize = (2 * n) + 2;
        int[]? pooledArray = null;

        Span<int> v = totalSize < 128
            ? stackalloc int[totalSize]
            : (pooledArray = ArrayPool<int>.Shared.Rent(totalSize));

        Span<int> v0 = v.Slice(0, n + 1);
        Span<int> v1 = v.Slice(n + 1, n + 1);

        // j is the current column
        // i is the current row
        // v0 is the previous row
        // v1 is the current row
        for (int j = 1; j <= n; j++)
            v0[j] = j;

        for (int i = 1; i <= m; i++)
        {
            v1[0] = i;
            for (int j = 1; j <= n; j++)
            {
                var deleteCost = v0[j] + 1;
                var insertCost = v1[j - 1] + 1;
                var substitutionCost = compare.Equals(s[i - 1], t[j - 1]) ? v0[j - 1] : v0[j - 1] + 1;
                v1[j] = Math.Min(deleteCost, Math.Min(insertCost, substitutionCost));
            }

            var temp = v0;
            v0 = v1;
            v1 = temp;
        }

        var result = v0[n];
        if (pooledArray != null)
            ArrayPool<int>.Shared.Return(pooledArray);
        return result;
    }

    public enum Operation
    {
        Start,
        Same,
        Delete,
        Insert,
        Substitute
    }

    private sealed record OperationNode(OperationNode? Predecessor, int Cost, int I, int J, Operation Operation)
    {
        public override string ToString()
            => Operation switch
            {
                Operation.Insert => $"Insert {J}",
                Operation.Delete => $"Delete {I}",
                Operation.Substitute => $"Substitute {I}->{J}",
                _ => Operation.ToString()
            };
    }

    public sealed record ResultOperation<T>(Operation Operation, T? OldValue, T? NewValue)
    {
        public override string ToString()
           => Operation switch
           {
               Operation.Insert => $"Insert {NewValue}",
               Operation.Delete => $"Delete {OldValue}",
               Operation.Substitute => $"Substitute {OldValue}->{NewValue}",
               _ => $"{Operation} {OldValue}={NewValue}"
           };
    }

    public static IReadOnlyList<ResultOperation<T>> CalculateEditSequence<T>(IReadOnlyList<T> s, IReadOnlyList<T> t, IEqualityComparer<T> compare)
    {
        if (s.Count == 0)
            return t.Select(x => new ResultOperation<T>(Operation.Insert, default, x)).ToList();
        if (t.Count == 0)
            return s.Select(x => new ResultOperation<T>(Operation.Delete, x, default)).ToList();
        int n = t.Count;
        int m = s.Count;
        int totalSize = (2 * n) + 2;

        OperationNode[] v = new OperationNode[totalSize];

        ArraySegment<OperationNode> v0 = new ArraySegment<OperationNode>(v, 0, n + 1);
        ArraySegment<OperationNode> v1 = new ArraySegment<OperationNode>(v, n + 1, n + 1);

        // j is the current column
        // i is the current row
        // v0 is the previous row
        // v1 is the current row
        for (int j = 0; j <= n; j++)
            v0[j] = new OperationNode(null, j, 0, j, Operation.Start);

        for (int i = 1; i <= m; i++)
        {
            v1[0] = new OperationNode(v0[0], i, i, 0, Operation.Same);
            for (int j = 1; j <= n; j++)
            {
                var deleteCost = v0[j].Cost + 1;
                var insertCost = v1[j - 1].Cost + 1;
                var sComparedToT = compare.Equals(s[i - 1], t[j - 1]);
                var substitutionCost = sComparedToT ? v0[j - 1].Cost : v0[j - 1].Cost + 1;
                if (deleteCost < insertCost && deleteCost < substitutionCost)
                    v1[j] = new OperationNode(v0[j], deleteCost, i, j, Operation.Delete);
                else if (insertCost < deleteCost && insertCost < substitutionCost)
                    v1[j] = new OperationNode(v1[j - 1], insertCost, i, j, Operation.Insert);
                else
                {
                    if (sComparedToT)
                        v1[j] = new OperationNode(v0[j - 1], v0[j - 1].Cost, i, j, Operation.Same);
                    else
                        v1[j] = new OperationNode(v0[j - 1], v0[j - 1].Cost + 1, i, j, Operation.Substitute);
                }
            }

            var temp = v0;
            v0 = v1;
            v1 = temp;
        }

        List<ResultOperation<T>> result = new List<ResultOperation<T>>();
        var current = v0[n];
        while (current != null && current!.Operation != Operation.Start)
        {
            if (current.Operation == Operation.Insert)
                result.Add(new ResultOperation<T>(current.Operation, default, t[current.J - 1]));
            else if (current.Operation == Operation.Delete)
                result.Add(new ResultOperation<T>(current.Operation, s[current.I - 1], default));
            else if (current.Operation == Operation.Substitute)
                result.Add(new ResultOperation<T>(current.Operation, s[current.I - 1], t[current.J - 1]));
            else if (current.Operation == Operation.Same)
                result.Add(new ResultOperation<T>(current.Operation, s[current.I - 1], t[current.J - 1]));
            current = current.Predecessor;
        }

        result.Reverse();
        return result;
    }
}
