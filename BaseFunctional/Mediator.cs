using Microsoft.Extensions.DependencyInjection;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public interface IRequest<in TResponse, out TError>;

public interface IHandler;

public interface IHandler<in TRequest, TResponse, TError> : IHandler
    where TRequest : IRequest<TResponse, TError>
{
    Result<TResponse, TError> Handle(TRequest request);
}

public interface IHandlerDecorator;

public interface IHandlerDecorator<TRequest, TResponse, TError> : IHandlerDecorator
{
    TRequest Before(TRequest request);

    Result<TResponse, TError> After(TRequest request, Result<TResponse, TError> result);
}

public sealed class DelegateHandler<TRequest, TResponse, TError> : IHandler<TRequest, TResponse, TError>
    where TRequest : IRequest<TResponse, TError>
{
    private readonly Func<TRequest, Result<TResponse, TError>> _func;

    public DelegateHandler(Func<TRequest, Result<TResponse, TError>> func)
    {
        _func = NotNull(func);
    }

    public Result<TResponse, TError> Handle(TRequest request) => _func(request);
}

public interface IMediator
{
    Result<TResponse, TError> Send<TRequest, TResponse, TError>(TRequest request)
        where TRequest : IRequest<TResponse, TError>;
}

public sealed class ServiceProviderMediator : IMediator
{
    private readonly IServiceProvider _provider;

    public ServiceProviderMediator(IServiceProvider provider)
    {
        _provider = NotNull(provider);
    }

    public Result<TResponse, TError> Send<TRequest, TResponse, TError>(TRequest request)
        where TRequest : IRequest<TResponse, TError>
    {
        var stages = _provider.GetServices<IHandlerDecorator<TRequest, TResponse, TError>>();
        request = stages.Aggregate(request, (prev, stage) => stage.Before(prev));
        var result = _provider.GetRequiredService<IHandler<TRequest, TResponse, TError>>().Handle(request);
        return stages.Reverse().Aggregate(result, (prev, stage) => stage.After(request, prev));
    }
}

public static class ServiceCollectionMediatorExtensions
{
    public static IServiceCollection UseMediator(this IServiceCollection services)
        => services.AddSingleton<IMediator, ServiceProviderMediator>();

    public static IServiceCollection AddHandlerSingleton<THandler>(this IServiceCollection services)
        where THandler : class, IHandler
    {
        foreach (var handlerInterface in typeof(THandler).GetHandlerInterfaces())
            services.AddSingleton(handlerInterface, typeof(THandler));
        return services;
    }

    public static IServiceCollection AddHandlerScoped<THandler>(this IServiceCollection services)
        where THandler : class, IHandler
    {
        foreach (var handlerInterface in typeof(THandler).GetHandlerInterfaces())
            services.AddScoped(handlerInterface, typeof(THandler));
        return services;
    }

    public static IServiceCollection AddHandlerTransient<THandler>(this IServiceCollection services)
        where THandler : class, IHandler
    {
        foreach (var handlerInterface in typeof(THandler).GetHandlerInterfaces())
            services.AddTransient(handlerInterface, typeof(THandler));
        return services;
    }

    public static IServiceCollection AddHandler<TRequest, TResponse, TError>(this IServiceCollection services, Func<TRequest, Result<TResponse, TError>> func)
        where TRequest : IRequest<TResponse, TError>
    {
        var handler = new DelegateHandler<TRequest, TResponse, TError>(func);
        return services.AddSingleton<IHandler<TRequest, TResponse, TError>>(_ => handler);
    }

    public static IServiceCollection AddHandlerDecoratorSingleton<TDecorator>(this IServiceCollection services)
        where TDecorator : class, IHandlerDecorator
    {
        foreach (var iface in typeof(TDecorator).GetHandlerDecoratorInterfaces())
            services.AddSingleton(iface, typeof(TDecorator));
        return services;
    }
}

public static class MediatorTypeExtensions
{
    public static IEnumerable<Type> GetHandlerInterfaces(this Type t)
        => GetInterfacesClosing(t, typeof(IHandler<,,>));

    public static IEnumerable<Type> GetHandlerDecoratorInterfaces(this Type t)
        => GetInterfacesClosing(t, typeof(IHandlerDecorator<,,>));

    public static IEnumerable<Type> GetInterfacesClosing(this Type t, Type openInterfaceType)
        => t.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);
}