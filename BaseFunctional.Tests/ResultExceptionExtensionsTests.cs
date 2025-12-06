namespace BaseFunctional.Tests;

public class ResultExceptionExtensionsTests
{
    [Test]
    public void MapExceptionToError_Default_WhenSuccess_ReturnsOriginalSuccess()
    {
        Result<string, Exception> result = (Result<string, Exception>)"ok";

        Result<string, Error> output = result.MapExceptionToError();

        output.IsSuccess.Should().BeTrue();
        output.GetValueOrDefault(string.Empty).Should().Be("ok");
    }

    [Test]
    public void MapExceptionToError_Default_WhenError_ProducesUnknownExceptionError()
    {
        Exception ex = new InvalidOperationException("boom");
        Result<string, Exception> result = (Result<string, Exception>)ex;

        Result<string, Error> output = result.MapExceptionToError();

        output.IsError.Should().BeTrue();
        Error err = output.GetErrorOrDefault(null!);
        err.Should().BeOfType<UnknownException>();
        ((UnknownException)err).Exception.Should().BeSameAs(ex);
        err.Message.Should().Contain("Unknown exception:");
        err.Message.Should().Contain(ex.GetType().Name);
        err.Message.Should().Contain(ex.Message);
    }

    [Test]
    public void MapExceptionToError_WithMapper_WhenSuccess_ReturnsOriginalSuccess()
    {
        Result<int, Exception> result = (Result<int, Exception>)42;
        IMap<Exception>.To<Error> mapper = new DelegateMapper<Exception, Error>(_ => new UnknownError("mapped"));

        Result<int, Error> output = result.MapExceptionToError(mapper);

        output.IsSuccess.Should().BeTrue();
        output.GetValueOrDefault(-1).Should().Be(42);
    }

    [Test]
    public void MapExceptionToError_WithMapper_WhenError_UsesProvidedMapper()
    {
        Exception ex = new ArgumentNullException("p");
        Result<string, Exception> result = (Result<string, Exception>)ex;
        RecordingMapper mapper = new RecordingMapper();

        Result<string, Error> output = result.MapExceptionToError(mapper);

        output.IsError.Should().BeTrue();
        mapper.Received.Should().BeSameAs(ex);
        Error err = output.GetErrorOrDefault(null!);
        err.Should().BeOfType<UnknownError>();
        err.Message.Should().Contain("mapped-" + ex.Message);
    }

    [Test]
    public void MapExceptionToError_WithMapper_NullMapper_ThrowsArgumentNullException()
    {
        Exception ex = new Exception("x");
        Result<string, Exception> result = (Result<string, Exception>)ex;

        Action act = () => result.MapExceptionToError(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class RecordingMapper : IMap<Exception>.To<Error>
    {
        public Exception? Received { get; private set; }

        public Error Map(Exception source)
        {
            Received = source;
            return new UnknownError("mapped-" + source.Message);
        }
    }
}
