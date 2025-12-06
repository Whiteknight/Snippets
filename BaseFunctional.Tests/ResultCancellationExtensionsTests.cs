namespace BaseFunctional.Tests;

public class ResultCancellationExtensionsTests
{
    [Test]
    public void CheckCancellation_WhenTokenNotCancelled_ReturnsOriginalSuccess()
    {
        var result = (Result<int, Error>)5;
        var output = result.CheckCancellation(CancellationToken.None);

        output.IsSuccess.Should().BeTrue();
        output.GetValueOrDefault(-1).Should().Be(5);
    }

    [Test]
    public void CheckCancellation_WhenTokenNotCancelled_ReturnsOriginalError()
    {
        var originalError = new UnknownError("original");
        var result = (Result<int, Error>)originalError;
        var output = result.CheckCancellation(CancellationToken.None);

        output.IsError.Should().BeTrue();
        output.GetErrorOrDefault(null!).Should().Be(originalError);
    }

    [Test]
    public void CheckCancellation_WhenTokenCancelled_ReplacesWithTaskCancelled_FromSuccess()
    {
        var result = (Result<int, Error>)7;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var output = result.CheckCancellation(cts.Token);

        output.IsError.Should().BeTrue();
        var err = output.GetErrorOrDefault(null!)!;
        err.Should().BeOfType<TaskCancelled>();
        err.Message.Should().Be("Task was cancelled.");
    }

    [Test]
    public void CheckCancellation_WhenTokenCancelled_ReplacesWithTaskCancelled_FromError()
    {
        var originalError = new UnknownError("orig");
        var result = (Result<int, Error>)originalError;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var output = result.CheckCancellation(cts.Token);

        output.IsError.Should().BeTrue();
        var err = output.GetErrorOrDefault(null!)!;
        err.Should().BeOfType<TaskCancelled>();
        err.Message.Should().Be("Task was cancelled.");
    }
}
