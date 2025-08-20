using Microsoft.Extensions.DependencyInjection;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public interface IRequest<in TResponse, out TResponseError>;

public interface IHandler<in TRequest, TResponse, TResponseError>
    where TRequest : IRequest<TResponse, TResponseError>
{
    Result<TResponse, TResponseError> Handle(TRequest request);
}

public interface IHandlerDecorator<TRequest, TResponse, TResponseError>
{
    void Before(TRequest request);
    void After(TRequest request, Result<TResponse, TResponseError> result);
}

public sealed class DelegateHandler<TRequest, TResponse, TResponseError> : IHandler<TRequest, TResponse, TResponseError>
    where TRequest : IRequest<TResponse, TResponseError>
{
    private readonly Func<TRequest, Result<TResponse, TResponseError>> _func;

    public DelegateHandler(Func<TRequest, Result<TResponse, TResponseError>> func)
    {
        _func = NotNull(func);
    }

    public Result<TResponse, TResponseError> Handle(TRequest request) => _func(request);
}

public interface IMediator
{
    Result<TResponse, TResponseError> Send<TRequest, TResponse, TResponseError>(TRequest request)
        where TRequest : IRequest<TResponse, TResponseError>;
}

public sealed class ServiceProviderMediator : IMediator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderMediator(IServiceProvider provider)
    {
        _provider = NotNull(provider);
    }

    public Result<TResponse, TResponseError> Send<TRequest, TResponse, TResponseError>(TRequest request)
        where TRequest : IRequest<TResponse, TResponseError>
    {
        var stages = _provider.GetServices<IHandlerDecorator<TRequest, TResponse, TResponseError>>();
        foreach (var stage in stages)
            stage.Before(request);
        var result = _provider.GetRequiredService<IHandler<TRequest, TResponse, TResponseError>>().Handle(request);
        foreach (var stage in stages.Reverse())
            stage.After(request, result);
        return result;
    }
}

public static class ServiceCollectionMediatorExtensions
{
    public static IServiceCollection UseMediator(this IServiceCollection services)
        => services.AddSingleton<IMediator, ServiceProviderMediator>();
    public static IServiceCollection AddHandlerTransient<THandler, TRequest, TResponse, TResponseError>(this IServiceCollection services)
        where THandler : class, IHandler<TRequest, TResponse, TResponseError>
        where TRequest : IRequest<TResponse, TResponseError>
    {
        return services.AddTransient<IHandler<TRequest, TResponse, TResponseError>, THandler>();
    }

    public static IServiceCollection AddHandlerTransient<TRequest, TResponse, TResponseError>(this IServiceCollection services, Func<TRequest, Result<TResponse, TResponseError>> func)
        where TRequest : IRequest<TResponse, TResponseError>
    {
        var handler = new DelegateHandler<TRequest, TResponse, TResponseError>(func);
        return services.AddTransient<IHandler<TRequest, TResponse, TResponseError>>(_ => handler);
    }

    public static IServiceCollection AddHandlerScoped<THandler, TRequest, TResponse, TResponseError>(this IServiceCollection services)
        where THandler : class, IHandler<TRequest, TResponse, TResponseError>
        where TRequest : IRequest<TResponse, TResponseError>
    {
        return services.AddScoped<IHandler<TRequest, TResponse, TResponseError>, THandler>();
    }

    public static IServiceCollection AddHandlerScoped<TRequest, TResponse, TResponseError>(this IServiceCollection services, Func<TRequest, Result<TResponse, TResponseError>> func)
        where TRequest : IRequest<TResponse, TResponseError>
    {
        var handler = new DelegateHandler<TRequest, TResponse, TResponseError>(func);
        return services.AddScoped<IHandler<TRequest, TResponse, TResponseError>>(_ => handler);
    }

    public static IServiceCollection AddHandlerSingleton<THandler, TRequest, TResponse, TResponseError>(this IServiceCollection services)
        where THandler : class, IHandler<TRequest, TResponse, TResponseError>
        where TRequest : IRequest<TResponse, TResponseError>
    {
        return services.AddSingleton<IHandler<TRequest, TResponse, TResponseError>, THandler>();
    }

    public static IServiceCollection AddHandlerSingleton<TRequest, TResponse, TResponseError>(this IServiceCollection services, Func<TRequest, Result<TResponse, TResponseError>> func)
        where TRequest : IRequest<TResponse, TResponseError>
    {
        var handler = new DelegateHandler<TRequest, TResponse, TResponseError>(func);
        return services.AddSingleton<IHandler<TRequest, TResponse, TResponseError>>(_ => handler);
    }
}