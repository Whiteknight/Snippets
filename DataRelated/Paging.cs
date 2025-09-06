namespace DataRelated;

using System.Collections;
using BaseFunctional;
using static BaseFunctional.Assert;

public readonly struct Paging : ICanBeValid, IEquatable<Paging>
{
    private Paging(int pageNumber, int startOffset, int pageSize)
    {
        Offset = startOffset < 0 ? 0 : startOffset;
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 1 : pageSize;
    }

    public static Paging Invalid => default;
    public static Paging Max => new Paging(1, 0, int.MaxValue);

    public int PageNumber { get; }
    public int PageSize { get; }
    public int Offset { get; }
    public bool IsValid => PageNumber >= 1 && PageSize >= 1;

    public static bool operator ==(Paging left, Paging right) => left.Equals(right);
    public static bool operator !=(Paging left, Paging right) => !(left == right);

    public static Paging FromPageNumberAndSize(int pageNumber, int size)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        size = size <= 0 ? 1 : size;
        var startOffset = (pageNumber - 1) * size;
        return new Paging(pageNumber, startOffset, size);
    }

    public static Paging FromOffsetAndSize(int offset, int size)
    {
        offset = offset < 0 ? 0 : offset;
        size = size <= 0 ? 1 : size;
        var pageNumber = offset / size + 1;
        return new Paging(pageNumber, offset, size);
    }

    public bool Equals(Paging other)
        => Offset == other.Offset
            && PageNumber == other.PageNumber
            && PageSize == other.PageSize;

    public override bool Equals(object? obj) => obj is Paging paging && Equals(paging);

    public override int GetHashCode() => HashCode.Combine(Offset, PageNumber, PageSize);

    public Paging ChangePageSize(int newSize)
    {
        InRange(newSize, 1, int.MaxValue);
        return IsValid
            ? FromOffsetAndSize(Offset, Math.Min(newSize, PageSize))
            : Invalid;
    }

    public Paging LimitPageSizeTo(int limit)
        => ChangePageSize(Math.Min(limit, PageSize));

    public static int CalculateTotalNumberOfPages(int size, int totalItems)
        => totalItems / size + (totalItems % size > 0 ? 1 : 0);
}

public readonly struct Page<T> : IEnumerable<T>, ICanBeValid
{
    private readonly IReadOnlyList<T>? _results;

    public Page(IReadOnlyList<T> results, int totalRecords, Paging paging)
    {
        _results = NotNull(results);
        TotalRecords = NotNegative(totalRecords);
        Paging = IsValid(paging);
        TotalPages = Paging.CalculateTotalNumberOfPages(paging.PageSize, TotalRecords);
    }

    public static Page<T> Invalid => new Page<T>([], 0, Paging.Invalid);

    public static Page<T> Empty => new Page<T>([], 0, Paging.FromPageNumberAndSize(1, 1));

    public bool IsValid => _results != null && Paging.IsValid;
    public int TotalRecords { get; }
    public int TotalPages { get; }
    public Paging Paging { get; }

    public bool IsOutOfRange => IsValid && TotalRecords > 0 && _results!.Count == 0;
    public IReadOnlyList<T> Results => _results ?? [];
    public static Page<T> OutOfRange(int total, Paging paging)
        => new Page<T>([], total, paging);
    public IEnumerator<T> GetEnumerator() => Results.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Page<TOut> ConvertAll<TOut>(Func<T, TOut> map)
        => IsValid
            ? new Page<TOut>(Results.Select(map).ToList(), TotalRecords, Paging)
            : Page<TOut>.Invalid;
}

public static class PagingQueryableExtensions
{
    public static IQueryable<T> Page<T>(this IQueryable<T> source, Paging paging)
    {
        IsValid(paging);
        return NotNull(source)
            .Skip(paging.Offset)
            .Take(paging.PageSize);
    }

    public static Page<T> ToPagedResult<T>(this IQueryable<T> query, Paging paging)
    {
        if (query is null || !paging.IsValid)
            return DataRelated.Page<T>.Invalid;

        var total = query.Count();
        if (total == 0)
            return DataRelated.Page<T>.Empty;

        IsValid(paging);
        var results = query.Page(paging).ToList();

        if (results.Count == 0)
            return DataRelated.Page<T>.OutOfRange(total, paging);

        return new Page<T>(results, total, paging);
    }
}

public static class PagingEnumerableExtensions
{
    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, Paging paging)
        => NotNull(source)
            .Skip(paging.Offset)
            .Take(paging.PageSize);
}
