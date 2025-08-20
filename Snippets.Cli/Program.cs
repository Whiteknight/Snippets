using BaseFunctional;
using Microsoft.Extensions.DependencyInjection;

namespace Snippets.Cli;

internal class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.UseMediator();
        services.AddHandlerSingleton<TestHandler, TestRequest, TestResponse, Exception>();
        services.AddSingleton<IHandlerDecorator<TestRequest, TestResponse, Exception>, TestDecorator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = mediator.Send<TestRequest, TestResponse, Exception>(new TestRequest(5));
        result.OnSuccess(v => Console.WriteLine(v));
    }
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
    public void After(TestRequest request, Result<TestResponse, Exception> result)
        => Console.WriteLine($"After: {request}, {result}");
    public void Before(TestRequest request) => Console.WriteLine($"Before: {request}");
}

