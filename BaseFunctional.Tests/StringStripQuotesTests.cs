namespace BaseFunctional.Tests;

public sealed class StringStripQuotesTests
{
    [Test]
    public void StripQuotes_Null_ReturnsEmptyOrSame()
    {
        string? input = null;
        var result = input.StripQuotes();
        result.Should().BeEmpty();
    }

    [Test]
    public void StripQuotes_Empty_ReturnsEmptyOrSame()
    {
        var input = "";
        var result = input.StripQuotes();
        result.Should().BeEmpty();
    }

    [Test]
    public void StripQuotes_LengthOne_ReturnsSame()
    {
        var input = "\"";
        var result = input.StripQuotes();
        result.Should().Be("\"");

        input = "'";
        result = input.StripQuotes();
        result.Should().Be("'");
    }

    [Test]
    public void StripQuotes_DoubleQuotes_Removed()
    {
        var input = "\"hello\"";
        var result = input.StripQuotes();
        result.Should().Be("hello");
    }

    [Test]
    public void StripQuotes_SingleQuotes_Removed()
    {
        var input = "'hello'";
        var result = input.StripQuotes();
        result.Should().Be("hello");
    }

    [Test]
    public void StripQuotes_NoQuotes_ReturnsSame()
    {
        var input = "hello";
        var result = input.StripQuotes();
        result.Should().Be("hello");
    }

    [Test]
    public void StripQuotes_MismatchedQuotes_ReturnsSame()
    {
        var input = "\"hello'";
        var result = input.StripQuotes();
        result.Should().Be("\"hello'");

        input = "'hello\"";
        result = input.StripQuotes();
        result.Should().Be("'hello\"");
    }

    [TestCase("\"he\\\"llo\"", "he\"llo")]
    [TestCase("'he\\'llo'", "he'llo")]
    [TestCase("\"a\\\\b\"", "a\\b")]
    [TestCase("\"a\\nb\\tc\"", "anbtc")]
    [TestCase("'a\\rb\\fc'", "arbfc")]
    [TestCase("a\\\"b", "a\"b")]
    public void StripQuotes_HandleBackslashEscapes_RemovesQuotesAndUnescapes(string input, string expected)
    {
        var result = input.StripQuotes(true);
        result.Should().Be(expected);
    }

    [Test]
    public void StripQuotes_HandleBackslashEscapes_HandlesEmpty()
    {
        string? input = null;
        var result = input.StripQuotes(true);
        result.Should().BeEmpty();

        input = "";
        result = input.StripQuotes(true);
        result.Should().BeEmpty();
    }
}
