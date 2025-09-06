namespace BaseFunctional.Tests;

public static class ResultTests
{
    public class CastOperators
    {
        [Test]
        public void Success()
        {
            var r = (Result<int, string>)42;
            r.IsSuccess.Should().BeTrue();
            r.GetValueOrDefault(-1).Should().Be(42);
        }

        [Test]
        public void Error()
        {
            var r = (Result<int, string>)"error";
            r.IsError.Should().BeTrue();
            r.GetErrorOrDefault("none").Should().Be("error");
        }
    }

    public class Match
    {
        [Test]
        public void Success()
        {
            var r = (Result<int, string>)5;
            var result = r.Match(i => i * 2, _ => -1);
            result.Should().Be(10);
        }

        [Test]
        public void Error()
        {
            var r = (Result<int, string>)"fail";
            var result = r.Match(i => i * 2, s => -2);
            result.Should().Be(-2);
        }

        [Test]
        public void WithData_Success()
        {
            var r = (Result<int, string>)7;
            var result = r.Match(3, (i, d) => i + d, (e, d) => -1);
            result.Should().Be(10);
        }

        [Test]
        public void WithData_Error()
        {
            var r = (Result<int, string>)"fail";
            var result = r.Match(3, (i, d) => i + d, (e, d) => -2);
            result.Should().Be(-2);
        }
    }

    public class Switch
    {
        [Test]
        public void Success()
        {
            var r = (Result<int, string>)1;
            var called = 0;
            r.Switch(i => called = i, e => called = -1);
            called.Should().Be(1);
        }

        [Test]
        public void Error()
        {
            var r = (Result<int, string>)"err";
            var called = 0;
            r.Switch(i => called = i, e => called = -2);
            called.Should().Be(-2);
        }

        [Test]
        public void WithData_Success()
        {
            var r = (Result<int, string>)2;
            var called = 0;
            r.Switch(3, (i, d) => called = i + d, (e, d) => called = -1);
            called.Should().Be(5);
        }

        [Test]
        public void WithData_Error()
        {
            var r = (Result<int, string>)"err";
            var called = 0;
            r.Switch(3, (i, d) => called = i + d, (e, d) => called = -2);
            called.Should().Be(-2);
        }
    }

    public class Bind
    {
        [Test]
        public void Success()
        {
            var r = (Result<int, string>)2;
            var r2 = r.Bind(i => Result.New<string, string>("ok"));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault("bad").Should().Be("ok");
        }

        [Test]
        public void Error()
        {
            var r = (Result<int, string>)"fail";
            var r2 = r.Bind(i => Result.New<string, string>("ok"));
            r2.IsError.Should().BeTrue();
            r2.GetErrorOrDefault("none").Should().Be("fail");
        }

        [Test]
        public void WithData_Success()
        {
            var r = (Result<int, string>)2;
            var r2 = r.Bind("x", (i, d) => Result.New<string, string>(d + i));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault("bad").Should().Be("x2");
        }
    }

    public class And
    {
        [Test]
        public void Success()
        {
            var r = (Result<int, string>)2;
            var r2 = r.And(i => Result.New<string, string>("ok"));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault("bad").Should().Be("ok");
        }

        [Test]
        public void WithMultipleErrors_Success()
        {
            var r = (Result<int, string>)2;
            var r2 = r.And<int, int, double>(i => new Result<int, int, double>(i + 1, default, default, 0));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault(-1).Should().Be(3);
        }
    }

    public class AndThen
    {
        [Test]
        public void CombinesErrors()
        {
            var r = (Result<int, string>)2;
            var r2 = r.AndThen<string, double>(i => new Result<string, double>("ok", default, 0));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault("bad").Should().Be("ok");
        }
    }

    public class Or
    {
        [Test]
        public void FirstSuccess_SecondError()
        {
            var r = (Result<int, string>)2;
            var r2 = r.Or(e => new Result<int, int>(-1, 1, 1));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault(-1).Should().Be(2);
        }

        [Test]
        public void FirstError_SecondSuccess()
        {
            var r3 = (Result<int, string>)"fail";
            var r4 = r3.Or(e => new Result<int, int>(1, -1, 0));
            r4.IsError.Should().BeFalse();
            r4.IsValid.Should().BeTrue();
            r4.GetValueOrDefault(0).Should().Be(1);
        }

        [Test]
        public void FirstError_SecondError()
        {
            var r3 = (Result<int, string>)"fail";
            var r4 = r3.Or(e => new Result<int, int>(-1, 1, 1));
            r4.IsError.Should().BeTrue();
            r4.GetErrorOrDefault(0).Should().Be(1);
        }
    }

    public class If
    {
        [Test]
        public void ExecutesThenWhenPredicateTrue()
        {
            var r = (Result<int, string>)2;
            var r2 = r.If(i => i > 1, i => (Result<int, string>)(i * 2));
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault(-1).Should().Be(4);
        }

        [Test]
        public void SkipsThenWhenPredicateFalse()
        {
            var r = (Result<int, string>)2;
            var r3 = r.If(i => i < 1, i => (Result<int, string>)(i * 2));
            r3.IsSuccess.Should().BeTrue();
            r3.GetValueOrDefault(-1).Should().Be(2);
        }
    }

    public class Map
    {
        [Test]
        public void MapsSuccessValue()
        {
            var r = (Result<int, string>)2;
            var r2 = r.Map(i => i * 3);
            r2.IsSuccess.Should().BeTrue();
            r2.GetValueOrDefault(-1).Should().Be(6);
        }
    }

    public class MapError
    {
        [Test]
        public void MapsErrorValue()
        {
            var r = (Result<int, string>)"fail";
            var r2 = r.MapError(e => e.Length);
            r2.IsError.Should().BeTrue();
            r2.GetErrorOrDefault(-1).Should().Be(4);
        }
    }

    public class OnSuccess
    {
        [Test]
        public void IsCalled()
        {
            var r = (Result<int, string>)2;
            var called = 0;
            r.OnSuccess(i => called = i);
            called.Should().Be(2);
        }
    }

    public class OnError
    {

        [Test]
        public void IsCalled()
        {
            var r = (Result<int, string>)"fail";
            var err = "";
            r.OnError(e => err = e);
            err.Should().Be("fail");
        }
    }

    public class GetValueOrDefault
    {
        [Test]
        public void GetErrorOrDefault()
        {
            var r = (Result<int, string>)2;
            r.GetValueOrDefault(-1).Should().Be(2);
            r.GetErrorOrDefault("none").Should().Be("none");

            var r2 = (Result<int, string>)"fail";
            r2.GetValueOrDefault(-1).Should().Be(-1);
            r2.GetErrorOrDefault("none").Should().Be("fail");
        }
    }

    public class Is
    {
        [Test]
        public void Equals_Works()
        {
            var r = (Result<int, string>)2;
            r.Is(2).Should().BeTrue();
            r.Is(3).Should().BeFalse();
            r.Is(i => i == 2).Should().BeTrue();
            r.Is(i => i == 3).Should().BeFalse();
            r.Equals(2).Should().BeTrue();
            r.Equals(3).Should().BeFalse();
        }
    }

    public class ToMaybe
    {
        [Test]
        public void ReturnsMaybe()
        {
            var r = (Result<int, string>)2;
            var maybe = r.ToMaybe();
            maybe.HasValue.Should().BeTrue();
            maybe.GetValueOrDefault(1).Should().Be(2);

            var r2 = (Result<int, string>)"fail";
            var maybe2 = r2.ToMaybe();
            maybe2.HasValue.Should().BeFalse();
        }
    }
}