namespace BaseFunctional.Tests;

public class OrEmptyIfNullTests
{
    [Test]
    public void WithNull_ReturnsEmpty()
    {
        IEnumerable<int>? source = null;
        var result = source.OrEmptyIfNull();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void WithEmptyEnumerable_ReturnsSameEnumerable()
    {
        var source = Enumerable.Empty<string>();
        var result = source.OrEmptyIfNull();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        result.Should().BeSameAs(source);
    }

    [Test]
    public void WithNonEmptyEnumerable_ReturnsSameEnumerable()
    {
        var source = new List<int> { 1, 2, 3 };
        var result = source.OrEmptyIfNull();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(source);
        result.Should().BeSameAs(source);
    }
}