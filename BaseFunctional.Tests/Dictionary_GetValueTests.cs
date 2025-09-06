namespace BaseFunctional.Tests;

public class Dictionary_GetValueTests
{
    [Test]
    public void Found()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        var m = dict.MaybeGetValue("a");
        m.HasValue.Should().BeTrue();
        m.GetValueOrDefault(-1).Should().Be(1);
    }

    [Test]
    public void NotFound()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };
        var m = dict.MaybeGetValue("b");
        m.HasValue.Should().BeFalse();
    }
}
