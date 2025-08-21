namespace BaseFunctional;

#pragma warning disable IDE1006 // Naming Styles

public abstract record MappingError(string Message);

public sealed record CannotMapNullObject() : MappingError("Source object cannot be null");

public static class IMap<TSource>
{
    public interface To<TTarget>
    {
        TTarget Map(TSource source);
    }

    public interface OnTo<TTarget>
    {
        void MapOnTo(TSource source, TTarget target);
    }
}

public sealed class OnToMappingAdaptor<TSource, TTarget> : IMap<TSource>.To<TTarget>
    where TTarget : new()
{
    private readonly IMap<TSource>.OnTo<TTarget> _mapper;

    public OnToMappingAdaptor(IMap<TSource>.OnTo<TTarget> mapper)
    {
        _mapper = mapper;
    }

    public TTarget Map(TSource source)
    {
        var target = new TTarget();
        _mapper.MapOnTo(source, target);
        return target;
    }
}

public static class ITryMap<TSource>
{
    public interface To<TTarget>
    {
        Result<TTarget, MappingError> Map(TSource source);
    }
}

public sealed class DelegateMapper<TSource, TTarget> : IMap<TSource>.To<TTarget>
{
    private readonly Func<TSource, TTarget> _map;

    public DelegateMapper(Func<TSource, TTarget> map)
    {
        _map = map;
    }

    public TTarget Map(TSource source) => _map(source);
}

public sealed class ResultDelegateMapper<TSource, TTarget> : ITryMap<TSource>.To<TTarget>
{
    private readonly Func<TSource, Result<TTarget, MappingError>> _map;

    public ResultDelegateMapper(Func<TSource, Result<TTarget, MappingError>> map)
    {
        _map = map;
    }

    public Result<TTarget, MappingError> Map(TSource source) => _map(source);
}

public sealed class ChainMapper<T1, T2, T3> : IMap<T1>.To<T3>
{
    private readonly IMap<T1>.To<T2> _map1;
    private readonly IMap<T2>.To<T3> _map2;

    public ChainMapper(IMap<T1>.To<T2> map1, IMap<T2>.To<T3> map2)
    {
        _map1 = map1;
        _map2 = map2;
    }

    public T3 Map(T1 source) => _map2.Map(_map1.Map(source));
}

public sealed class ResultChainMapper<T1, T2, T3> : ITryMap<T1>.To<T3>
{
    private readonly ITryMap<T1>.To<T2> _map1;
    private readonly ITryMap<T2>.To<T3> _map2;

    public ResultChainMapper(ITryMap<T1>.To<T2> map1, ITryMap<T2>.To<T3> map2)
    {
        _map1 = map1;
        _map2 = map2;
    }

    public Result<T3, MappingError> Map(T1 source)
        => _map1.Map(source)
            .And(_map2.Map);
}

public static class MappingExtensions
{
    public static IMap<TIn>.To<TOut2> Chain<TIn, TOut1, TOut2>(this IMap<TIn>.To<TOut1> map1, IMap<TOut1>.To<TOut2> map2)
        => new ChainMapper<TIn, TOut1, TOut2>(map1, map2);

    public static ITryMap<TIn>.To<TOut2> Chain<TIn, TOut1, TOut2>(this ITryMap<TIn>.To<TOut1> map1, ITryMap<TOut1>.To<TOut2> map2)
        => new ResultChainMapper<TIn, TOut1, TOut2>(map1, map2);
}
