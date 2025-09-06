namespace BaseFunctional.Tests;

public class ResultMatchAsync_Result_Async_NoData_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess()
    {
        var result = new Result<int, string>(42, default, 0);
        var output = await result.MatchAsync(
            async (v, ct) => { await Task.Delay(1, ct); return v * 2; },
            async (e, ct) => { await Task.Delay(1, ct); return -1; },
            CancellationToken.None);
        output.Should().Be(84);
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError()
    {
        var result = new Result<int, string>(default, "fail", 1);
        var called = false;
        var output = await result.MatchAsync(
            async (v, ct) => { await Task.Delay(1, ct); return 1; },
            async (e, ct) => { called = true; await Task.Delay(1, ct); return -2; },
            CancellationToken.None);
        called.Should().BeTrue();
        output.Should().Be(-2);
    }
}

public class ResultMatchAsync_Result_Async_Data_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess_WithData()
    {
        var result = new Result<int, string>(10, default, 0);
        var output = await result.MatchAsync(
            "data",
            async (v, d, ct) => { await Task.Delay(1, ct); return $"{v}-{d}"; },
            async (e, d, ct) => { await Task.Delay(1, ct); return "err"; },
            CancellationToken.None);
        output.Should().Be("10-data");
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError_WithData()
    {
        var result = new Result<int, string>(default, "fail", 1);
        var output = await result.MatchAsync(
            "data",
            async (v, d, ct) => { await Task.Delay(1, ct); return "ok"; },
            async (e, d, ct) => { await Task.Delay(1, ct); return $"{e}-{d}"; },
            CancellationToken.None);
        output.Should().Be("fail-data");
    }
}

public class ResultMatchAsync_TaskResult_Sync_NoData_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess()
    {
        var result = Task.FromResult(new Result<int, string>(7, default, 0));
        var output = await result.MatchAsync(v => v + 1, e => -1);
        output.Should().Be(8);
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError()
    {
        var result = Task.FromResult(new Result<int, string>(default, "fail", 1));
        var output = await result.MatchAsync(v => v + 1, e => -2);
        output.Should().Be(-2);
    }
}

public class ResultMatchAsync_TaskResult_Sync_Data_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess_WithData()
    {
        var result = Task.FromResult(new Result<int, string>(3, default, 0));
        var output = await result.MatchAsync(
            5,
            (v, d) => v * d,
            (e, d) => -1);
        output.Should().Be(15);
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError_WithData()
    {
        var result = Task.FromResult(new Result<int, string>(default, "fail", 1));
        var output = await result.MatchAsync(
            5,
            (v, d) => v * d,
            (e, d) => -2);
        output.Should().Be(-2);
    }
}

public class ResultMatchAsync_TaskResult_Async_NoData_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess_Async()
    {
        var result = Task.FromResult(new Result<int, string>(8, default, 0));
        var output = await result.MatchAsync(
            async (v, ct) => { await Task.Delay(1, ct); return v * 10; },
            async (e, ct) => { await Task.Delay(1, ct); return -1; },
            CancellationToken.None);
        output.Should().Be(80);
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError_Async()
    {
        var result = Task.FromResult(new Result<int, string>(default, "fail", 1));
        var output = await result.MatchAsync(
            async (v, ct) => { await Task.Delay(1, ct); return 1; },
            async (e, ct) => { await Task.Delay(1, ct); return -2; },
            CancellationToken.None);
        output.Should().Be(-2);
    }
}

public class ResultMatchAsync_TaskResult_Async_Data_Tests
{
    [Test]
    public async Task MatchAsync_Success_CallsOnSuccess_Async_WithData()
    {
        var result = Task.FromResult(new Result<int, string>(9, default, 0));
        var output = await result.MatchAsync(
            "x",
            async (v, d, ct) => { await Task.Delay(1, ct); return $"{v}-{d}"; },
            async (e, d, ct) => { await Task.Delay(1, ct); return "err"; },
            CancellationToken.None);
        output.Should().Be("9-x");
    }

    [Test]
    public async Task MatchAsync_Error_CallsOnError_Async_WithData()
    {
        var result = Task.FromResult(new Result<int, string>(default, "fail", 1));
        var output = await result.MatchAsync(
            "x",
            async (v, d, ct) => { await Task.Delay(1, ct); return "ok"; },
            async (e, d, ct) => { await Task.Delay(1, ct); return $"{e}-{d}"; },
            CancellationToken.None);
        output.Should().Be("fail-x");
    }
}