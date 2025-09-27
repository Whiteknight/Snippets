using BaseFunctional;
using static BaseFunctional.Assert;

namespace DataRelated;

/* Create a new cursor to start the load
 * 
 *      var cursor = Cursor.StartInt(50);
 *      
 * Get Items from the data source and create a CursorPage
 * 
 *      var items = dataStore.GetItems(cursor);
 *      var page = new CursorPage(cursor, items, item => item.Id);
 *      
 * Get the next page of items from the data source
 * 
 *      var nextItems = dataStore.GetItems(page.NextCursor);
 */

public static class Cursor
{
    public static Cursor<int> StartInt(int pageSize)
        => new(default!, pageSize, IsIntValid);

    public static Cursor<long> StartLong(int pageSize)
        => new(default!, pageSize, IsLongValid);

    public static Cursor<string> StartString(int pageSize)
        => new(default!, pageSize, IsStringValid);

    public static Cursor<Guid> StartGuid(int pageSize)
        => new(default!, pageSize, IsGuidValid);

    public static Cursor<TKey> Start<TKey>(int pageSize, Func<TKey, bool> isKeyValid)
        => new(default!, pageSize, NotNull(isKeyValid));

    private static bool IsIntValid(int key) => key > 0;

    private static bool IsLongValid(long key) => key > 0;

    private static bool IsStringValid(string key) => !string.IsNullOrWhiteSpace(key);

    private static bool IsGuidValid(Guid key) => key != Guid.Empty;
}

public readonly struct Cursor<TKey> : ICanBeValid
{
    private readonly Func<TKey, bool> _isKeyValid;

    public Cursor(TKey? lastKey, int pageSize, Func<TKey, bool> isKeyValid)
    {
        LastKey = lastKey;
        _isKeyValid = NotNull(isKeyValid);
        PageSize = pageSize <= 0 ? 1 : pageSize;
    }

    public bool IsValid => PageSize > 0 && LastKey is not null && _isKeyValid(LastKey!);

    public bool IsStart => PageSize > 0 && (LastKey is null || !_isKeyValid(LastKey));

    public TKey? LastKey { get; }

    public int PageSize { get; }

    public Cursor<TKey> Next(TKey nextKey)
        => new Cursor<TKey>(nextKey, PageSize, _isKeyValid);

    public TOut Match<TOut>(Func<TOut> onStart, Func<TKey, TOut> onMiddle, Func<TOut> onInvalid)
    {
        if (IsStart)
            return NotNull(onStart)();
        if (IsValid)
            return NotNull(onMiddle)(LastKey!);
        return NotNull(onInvalid)();
    }

    public TOut Match<TOut, TData>(TData data, Func<TData, TOut> onStart, Func<TKey, TData, TOut> onMiddle, Func<TData, TOut> onInvalid)
    {
        if (IsStart)
            return NotNull(onStart)(data);
        if (IsValid)
            return NotNull(onMiddle)(LastKey!, data);
        return NotNull(onInvalid)(data);
    }

    public void Switch(Action onStart, Action<TKey> onMiddle, Action onInvalid)
    {
        if (IsStart)
        {
            NotNull(onStart)();
            return;
        }

        if (IsValid)
        {
            NotNull(onMiddle)(LastKey!);
            return;
        }

        NotNull(onInvalid)();
    }

    public void Switch<TData>(TData data, Action<TData> onStart, Action<TKey, TData> onMiddle, Action<TData> onInvalid)
    {
        if (IsStart)
        {
            NotNull(onStart)(data);
            return;
        }

        if (IsValid)
        {
            NotNull(onMiddle)(LastKey!, data);
            return;
        }

        NotNull(onInvalid)(data);
    }
}

public readonly struct CursorPage<TKey, TValue> : ICanBeValid
{
    public CursorPage(Cursor<TKey> thisCursor, IReadOnlyList<TValue> items, Func<TValue, TKey> getNextCursor)
    {
        ThisCursor = thisCursor;
        Items = items;
        NextCursor = items == null || items.Count < ThisCursor.PageSize
            ? default
            : ThisCursor.Next(getNextCursor(items[^1]));
    }

    public CursorPage(Cursor<TKey> thisCursor, IReadOnlyList<TValue> items, Cursor<TKey> nextCursor)
    {
        ThisCursor = thisCursor;
        Items = items;
        NextCursor = nextCursor;
    }

    public CursorPage(Cursor<TKey> thisCursor, IReadOnlyList<TValue> items)
    {
        ThisCursor = thisCursor;
        Items = items;
        NextCursor = default;
    }

    public Cursor<TKey> ThisCursor { get; }

    public Cursor<TKey> NextCursor { get; }

    public IReadOnlyList<TValue> Items { get; }

    public bool IsValid => ThisCursor.IsValid && Items is not null;

    public bool HasMore => Items.Count <= ThisCursor.PageSize && NextCursor.IsValid;

    public CursorPage<TKey, TOut> ConvertAll<TOut>(Func<TValue, TOut> converter)
        => new(ThisCursor, Items.Select(converter).ToList(), NextCursor);
}
