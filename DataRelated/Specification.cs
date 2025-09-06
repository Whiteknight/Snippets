using BaseFunctional;

namespace DataRelated;

public interface IQuerySpecification<T>
{
    IQueryable<T> Apply(IQueryable<T> query);
}

public sealed class CombinedQuerySpecification<T> : IQuerySpecification<T>
{
    public CombinedQuerySpecification(IEnumerable<IQuerySpecification<T>> specs)
    {
        Specifications = specs.OrEmptyIfNull().ToArray();
    }

    public IQuerySpecification<T>[] Specifications { get; }

    public IQueryable<T> Apply(IQueryable<T> query)
        => Specifications.Aggregate(query, (q, spec) => spec.Apply(q));
}

// Spec that returns no results. Useful in, for example, security contexts where a user is not
// permitted to view items of a certain type.
public sealed class NothingQuerySpecification<T> : IQuerySpecification<T>
{
    public IQueryable<T> Apply(IQueryable<T> query) => query.Where(_ => 1 == 2);
}

// Specification which returns everything
public sealed class PermissiveQuerySpecification<T> : IQuerySpecification<T>
{
    public IQueryable<T> Apply(IQueryable<T> query) => query;
}

public static class Specifications
{
    public static IQuerySpecification<T> Combine<T>(params IQuerySpecification<T>[] specs)
    {
        var newSpecs = new List<IQuerySpecification<T>>();
        PermissiveQuerySpecification<T>? permissive = null;

        foreach (var spec in specs)
        {
            if (spec is NothingQuerySpecification<T>)
                return spec;

            if (spec is PermissiveQuerySpecification<T> perm)
            {
                // Keep track of this instance, just in case we need one and don't want to re-allocate
                permissive ??= perm;
                continue;
            }

            if (spec is CombinedQuerySpecification<T> combined)
            {
                newSpecs.AddRange(combined.Specifications);
                continue;
            }

            newSpecs.Add(spec);
        }

        return newSpecs switch
        {
            [] => permissive ?? new PermissiveQuerySpecification<T>(),
            [IQuerySpecification<T> only] => only,
            [..] => new CombinedQuerySpecification<T>(newSpecs)
        };
    }
}
