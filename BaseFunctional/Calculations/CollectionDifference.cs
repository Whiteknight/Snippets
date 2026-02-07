using static BaseFunctional.Assert;

namespace BaseFunctional.Calculations;

public static class CollectionDifference
{
    public readonly record struct Difference<T>(bool AreIdentical, IReadOnlyList<Item<T>> Changes)
    {
        public static Difference<T> New(List<Item<T>> changes)
        {
            var identical = changes.Count == 0 || changes.All(i => i.Type == ChangeType.Unchanged);
            return new Difference<T>(identical, changes);
        }
    }

    public readonly record struct Item<T>(ChangeType Type, T? Old, T? New)
    {
        public static Item<T> Insert(T newValue) => new Item<T>(ChangeType.Inserted, default, newValue);

        public static Item<T> Remove(T oldValue) => new Item<T>(ChangeType.Removed, oldValue, default);

        public static Item<T> Unchanged(T oldValue, T newValue) => new Item<T>(ChangeType.Unchanged, oldValue, newValue);
    }

    public enum ChangeType
    {
        Inserted,
        Removed,
        Unchanged
    }

    public static Difference<T> Calculate<T, TKey>(ICollection<T> oldItems, ICollection<T> newItems, Func<T, TKey> getKey)
        where TKey : notnull
    {
        var items = CalculateInternal(NotNull(oldItems), NotNull(newItems), NotNull(getKey));
        return Difference<T>.New(items);
    }

    private static List<Item<T>> CalculateInternal<T, TKey>(ICollection<T> source, ICollection<T> target, Func<T, TKey> getUniqueKey)
        where TKey : notnull
    {
        var targetItems = target.ToDictionary(getUniqueKey);

        // Degenerate cases. If one or both of the sets is empty we can say all items are added/removed
        if (source.Count == 0)
            return targetItems.Count == 0 ? [] : EntireCollectionInserted(targetItems.Values);
        if (targetItems.Count == 0)
            return EntireCollectionDeleted(source);

        var difference = new List<Item<T>>(source.Count);

        foreach (var item in source)
        {
            var uniqueKey = getUniqueKey(item);

            // Same item exists in both.
            if (targetItems.TryGetValue(uniqueKey, out var newValue))
            {
                targetItems.Remove(uniqueKey);
                difference.Add(Item<T>.Unchanged(item, newValue));
                continue;
            }

            // Item is in source but not target.
            difference.Add(Item<T>.Remove(item));
        }

        // Items still in the target list but not seen in source are new.
        foreach (var (_, value) in targetItems)
            difference.Add(Item<T>.Insert(value));

        return difference;
    }

    private static List<Item<T>> EntireCollectionDeleted<T>(IEnumerable<T> values)
        => [.. values.Select(Item<T>.Remove)];

    private static List<Item<T>> EntireCollectionInserted<T>(IEnumerable<T> values)
        => [.. values.Select(Item<T>.Insert)];
}
