namespace BaseFunctional.Tests;

public class EitherTests
{
    [Test]
    public void ImplicitLeftOperator_CreatesLeft()
    {
        Either<int, string> e = (Either<int, string>)5;
        e.IsLeft.Should().BeTrue();
        e.IsRight.Should().BeFalse();
        e.GetLeftOrDefault(-1).Should().Be(5);
        e.GetRightOrDefault("none").Should().Be("none");
    }

    [Test]
    public void ImplicitRightOperator_CreatesRight()
    {
        Either<int, string> e = (Either<int, string>)"err";
        e.IsRight.Should().BeTrue();
        e.IsLeft.Should().BeFalse();
        e.GetRightOrDefault("none").Should().Be("err");
        e.GetLeftOrDefault(-1).Should().Be(-1);
    }

    [Test]
    public void FromLeft_FromRight_Factories_Work()
    {
        Either<int, string> left = Either.FromLeft<int, string>(10);
        Either<int, string> right = Either.FromRight<int, string>("r");

        left.IsLeft.Should().BeTrue();
        left.GetLeftOrDefault(0).Should().Be(10);

        right.IsRight.Should().BeTrue();
        right.GetRightOrDefault(string.Empty).Should().Be("r");
    }

    [Test]
    public void Match_ReturnsLeftOrRight()
    {
        Either<int, string> left = (Either<int, string>)3;
        int leftResult = left.Match(i => i * 2, s => -1);
        leftResult.Should().Be(6);

        Either<int, string> right = (Either<int, string>)"bad";
        int rightResult = right.Match(i => i * 2, s => -2);
        rightResult.Should().Be(-2);
    }

    [Test]
    public void Match_WithData_ReturnsExpected()
    {
        Either<int, string> left = (Either<int, string>)4;
        string outLeft = left.Match("d", (v, d) => $"{v}-{d}", (r, d) => "err");
        outLeft.Should().Be("4-d");

        Either<int, string> right = (Either<int, string>)"no";
        string outRight = right.Match("d", (v, d) => "ok", (r, d) => $"{r}-{d}");
        outRight.Should().Be("no-d");
    }

    [Test]
    public void Switch_InvokesCorrectAction()
    {
        Either<int, string> left = (Either<int, string>)7;
        int leftCalled = 0;
        left.Switch(i => leftCalled = i, _ => leftCalled = -1);
        leftCalled.Should().Be(7);

        Either<int, string> right = (Either<int, string>)"z";
        string rightCalled = string.Empty;
        right.Switch(_ => { }, s => rightCalled = s);
        rightCalled.Should().Be("z");
    }

    [Test]
    public void MapLeft_MapRight_PreserveOppositeSide()
    {
        Either<int, string> left = (Either<int, string>)2;
        Either<int, string> mappedLeft = left.MapLeft(i => i * 3);
        mappedLeft.IsLeft.Should().BeTrue();
        mappedLeft.GetLeftOrDefault(0).Should().Be(6);

        Either<int, string> right = (Either<int, string>)"hi";
        Either<int, string> mappedRight = right.MapRight(s => s + "!");
        mappedRight.IsRight.Should().BeTrue();
        mappedRight.GetRightOrDefault(string.Empty).Should().Be("hi!");
    }

    [Test]
    public void OnLeft_OnRight_ReturnOriginalAndInvokeSideEffect()
    {
        Either<int, string> left = (Either<int, string>)1;
        int seen = 0;
        Either<int, string> retLeft = left.OnLeft(i => seen = i);
        retLeft.IsLeft.Should().BeTrue();
        seen.Should().Be(1);

        Either<int, string> right = (Either<int, string>)"x";
        string seenRight = string.Empty;
        Either<int, string> retRight = right.OnRight(s => seenRight = s);
        retRight.IsRight.Should().BeTrue();
        seenRight.Should().Be("x");
    }

    [Test]
    public void GetLeftOrDefault_GetRightOrDefault_BehaveCorrectly()
    {
        Either<int, string> left = (Either<int, string>)9;
        left.GetLeftOrDefault(-1).Should().Be(9);
        left.GetRightOrDefault("none").Should().Be("none");

        Either<int, string> right = (Either<int, string>)"ok";
        right.GetRightOrDefault("no").Should().Be("ok");
        right.GetLeftOrDefault(-5).Should().Be(-5);
    }

    [Test]
    public void LeftIs_RightIs_WithPredicates_AndEquals()
    {
        Either<int, string> left = (Either<int, string>)5;
        left.LeftIs(5).Should().BeTrue();
        left.LeftIs(i => i > 3).Should().BeTrue();
        left.LeftIs(i => i < 0).Should().BeFalse();

        Either<int, string> right = (Either<int, string>)"abc";
        right.RightIs("abc").Should().BeTrue();
        right.RightIs(s => s.StartsWith("a")).Should().BeTrue();
        right.RightIs(s => s.Length == 0).Should().BeFalse();
    }

    [Test]
    public void Invert_SwitchesSides()
    {
        Either<int, string> left = (Either<int, string>)21;
        Either<string, int> invLeft = left.Invert();
        invLeft.IsRight.Should().BeTrue();
        invLeft.GetRightOrDefault(0).Should().Be(21);

        Either<int, string> right = (Either<int, string>)"v";
        Either<string, int> invRight = right.Invert();
        invRight.IsLeft.Should().BeTrue();
        invRight.GetLeftOrDefault(string.Empty).Should().Be("v");
    }

    [Test]
    public void DefaultStruct_IsInvalid_ThrowsOnMatchAndSwitch()
    {
        Either<object, string> def = default;
        Action matchAct = () => def.Match(i => i, s => s);
        matchAct.Should().Throw<InvalidOperationException>();

        Action switchAct = () => def.Switch(i => { }, s => { });
        switchAct.Should().Throw<InvalidOperationException>();
    }
}
