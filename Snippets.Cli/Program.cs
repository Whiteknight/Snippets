using BaseFunctional;
using Microsoft.Extensions.DependencyInjection;

namespace Snippets.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.UseMediator();
        //services.AddHandlerSingleton<TestHandler>();
        services.AddHandler<TestRequest, TestResponse, Exception>(HandlerFunction);
        services.AddHandlerDecoratorSingleton<TestDecorator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = mediator.Send(new TestRequest(5));
        result.OnSuccess(v => Console.WriteLine(v));
    }

    private static Result<TestResponse, Exception> HandlerFunction(TestRequest request)
        => new TestResponse($"Function Value is {request.Value}");
}

public sealed record TestResponse(string Value);

public sealed record TestRequest(int Value) : IRequest<TestResponse, Exception>;

public sealed class TestHandler : IHandler<TestRequest, TestResponse, Exception>
{
    public Result<TestResponse, Exception> Handle(TestRequest request)
        => new TestResponse($"Value is {request.Value}");
}

public sealed class TestDecorator : IHandlerDecorator<TestRequest, TestResponse, Exception>
{
    public Result<TestResponse, Exception> After(TestRequest request, Result<TestResponse, Exception> result)
    {
        Console.WriteLine($"After: {request}, {result}");
        return result.Map(v => new TestResponse($"Decorated {v.Value}"));
    }

    public TestRequest Before(TestRequest request)
    {
        Console.WriteLine($"Before: {request}");
        return new TestRequest(request.Value + 1);
    }
}
