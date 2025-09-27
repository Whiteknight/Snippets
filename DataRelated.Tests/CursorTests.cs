namespace DataRelated.Tests;

public class CursorTests
{
    [Test]
    public void StartInt_ShouldCreateStartCursor_WithDefaultKeyAndPageSize()
    {
        var cursor = Cursor.StartInt(10);

        cursor.PageSize.Should().Be(10);
        cursor.LastKey.Should().Be(0);
        cursor.IsStart.Should().BeTrue();
        cursor.IsValid.Should().BeFalse();
    }

    [Test]
    public void Cursor_IsValid_ShouldBeTrue_WhenKeyIsValidAndPageSizePositive()
    {
        var cursor = new Cursor<int>(5, 10, k => k > 0);

        cursor.IsValid.Should().BeTrue();
        cursor.IsStart.Should().BeFalse();
    }

    [Test]
    public void Cursor_IsValid_ShouldBeFalse_WhenKeyIsInvalid()
    {
        var cursor = new Cursor<int>(0, 10, k => k > 0);

        cursor.IsValid.Should().BeFalse();
        cursor.IsStart.Should().BeTrue();
    }

    [Test]
    public void Cursor_Next_ShouldReturnCursorWithNewKey()
    {
        var cursor = new Cursor<int>(1, 10, k => k > 0);
        var next = cursor.Next(2);

        next.LastKey.Should().Be(2);
        next.PageSize.Should().Be(10);
        next.IsValid.Should().BeTrue();
    }

    [Test]
    public void Cursor_Match_Start()
    {
        var cursor = new Cursor<int>(0, 10, k => k > 0);

        var result = cursor.Match(
            () => "start",
            k => "middle",
            () => "invalid"
        );

        result.Should().Be("start");
    }

    [Test]
    public void Cursor_Match_ShouldReturnCorrectBranch()
    {
        var cursor = new Cursor<int>(5, 10, k => k > 0);

        var result = cursor.Match(
            () => "start",
            k => "middle",
            () => "invalid"
        );

        result.Should().Be("middle");
    }

    [Test]
    public void Cursor_Match_Invalid()
    {
        Cursor<int> cursor = default;

        var result = cursor.Match(
            () => "start",
            k => "middle",
            () => "invalid"
        );

        result.Should().Be("invalid");
    }
}
