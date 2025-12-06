using System.Linq.Expressions;

namespace DataRelated;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);

    IQueryable<T> Apply(IQueryable<T> query);
}

public sealed class CombinedSpecification<T> : ISpecification<T>
{
    private CombinedSpecification(IReadOnlyList<ISpecification<T>> specs)
    {
        Specifications = specs;
    }

    public static ISpecification<T> Create(ISpecification<T>[] specs)
    {
        if (specs == null || specs.Length == 0)
            return PermissiveSpecification<T>.Instance;
        if (specs.Length == 1)
            return specs[0];

        var newSpecs = new List<ISpecification<T>>();
        foreach (var spec in specs)
        {
            if (spec is FailureSpecification<T>)
                return spec;

            if (spec is PermissiveSpecification<T>)
                continue;

            if (spec is CombinedSpecification<T> combined)
            {
                newSpecs.AddRange(combined.Specifications);
                continue;
            }

            newSpecs.Add(spec);
        }

        return newSpecs switch
        {
            [] => PermissiveSpecification<T>.Instance,
            [ISpecification<T> only] => only,
            [..] => new CombinedSpecification<T>(newSpecs)
        };
    }

    public IReadOnlyList<ISpecification<T>> Specifications { get; }

    public IQueryable<T> Apply(IQueryable<T> query)
        => Specifications.Aggregate(query, (q, spec) => spec.Apply(q));

    public bool IsSatisfiedBy(T entity)
    {
        for (int i = 0; i < Specifications.Count; i++)
        {
            if (!Specifications[i].IsSatisfiedBy(entity))
                return false;
        }

        return true;
    }
}

// Spec that returns no results. Useful in, for example, security contexts where a user is not
// permitted to view items of a certain type.
public sealed class FailureSpecification<T> : ISpecification<T>
{
    private FailureSpecification()
    {
    }

    public static FailureSpecification<T> Instance { get; } = new FailureSpecification<T>();

    public IQueryable<T> Apply(IQueryable<T> query) => query.Where(_ => 1 == 2);

    public bool IsSatisfiedBy(T entity) => false;
}

// Specification which returns everything
public sealed class PermissiveSpecification<T> : ISpecification<T>
{
    private PermissiveSpecification()
    {
    }

    public static PermissiveSpecification<T> Instance { get; } = new PermissiveSpecification<T>();

    public IQueryable<T> Apply(IQueryable<T> query) => query;

    public bool IsSatisfiedBy(T entity) => true;
}

public sealed class ExpressionSpecification<T> : ISpecification<T>
{
    private readonly Expression<Func<T, bool>> _expression;
    private readonly Func<T, bool> _compiled;

    public ExpressionSpecification(Expression<Func<T, bool>> expression)
    {
        _expression = expression;
        _compiled = expression.Compile();
    }

    public IQueryable<T> Apply(IQueryable<T> query) => query.Where(_expression);

    public bool IsSatisfiedBy(T entity) => _compiled(entity);
}

public static class Specifications
{
    public static ISpecification<T> Combine<T>(params ISpecification<T>[] specs)
        => CombinedSpecification<T>.Create(specs);

    public static ISpecification<T> Fail<T>()
        => FailureSpecification<T>.Instance;

    public static ISpecification<T> PermitAll<T>()
        => PermissiveSpecification<T>.Instance;

    public static ISpecification<T> From<T>(Expression<Func<T, bool>> expression)
        => new ExpressionSpecification<T>(expression);
}
