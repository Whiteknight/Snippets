using AwesomeAssertions;

namespace BaseFunctional.Tests;

public static class MaybeTests
{
    public class Cast
    {
        [Test]
        public void SetsHasValue()
        {
            var m = (Maybe<int>)42;
            m.HasValue.Should().BeTrue();
            m.GetValueOrDefault(-1).Should().Be(42);
        }
    }

    public class HasValue
    {
        [Test]
        public void True_WhenConstructedWithValue()
        {
            var m = new Maybe<string>("abc", true);
            m.HasValue.Should().BeTrue();
        }

        [Test]
        public void False_WhenDefault()
        {
            var m = default(Maybe<string>);
            m.HasValue.Should().BeFalse();
        }
    }

    public class Match
    {
        [Test]
        public void OnValue_CallsOnValue()
        {
            var m = (Maybe<int>)5;
            var result = m.Match(i => i * 2, () => -1);
            result.Should().Be(10);
        }

        [Test]
        public void OnNoValue_CallsOnNoValue()
        {
            var m = default(Maybe<int>);
            var result = m.Match(i => i * 2, () => -2);
            result.Should().Be(-2);
        }

        [Test]
        public void WithData_OnValue()
        {
            var m = (Maybe<int>)7;
            var result = m.Match(3, (i, d) => i + d, d => -1);
            result.Should().Be(10);
        }

        [Test]
        public void WithData_OnNoValue()
        {
            var m = default(Maybe<int>);
            var result = m.Match(3, (i, d) => i + d, d => -2);
            result.Should().Be(-2);
        }
    }

    public class ToResult
    {
        [Test]
        public void WithValue_ReturnsSuccess()
        {
            var m = (Maybe<int>)5;
            var r = m.ToResult(() => "err");
            r.IsSuccess.Should().BeTrue();
            r.GetValueOrDefault(-1).Should().Be(5);
        }

        [Test]
        public void NoValue_ReturnsError()
        {
            var m = default(Maybe<int>);
            var r = m.ToResult(() => "err");
            r.IsError.Should().BeTrue();
            r.GetErrorOrDefault("none").Should().Be("err");
        }
    }

    public class GetValueOrDefault
    {
        [Test]
        public void WithValue()
        {
            var m = (Maybe<int>)7;
            m.GetValueOrDefault(-1).Should().Be(7);
        }

        [Test]
        public void NoValue()
        {
            var m = default(Maybe<int>);
            m.GetValueOrDefault(-1).Should().Be(-1);
        }
    }

    public class Is
    {
        [Test]
        public void WithPredicate_True()
        {
            var m = (Maybe<int>)5;
            m.Is(i => i == 5).Should().BeTrue();
        }

        [Test]
        public void WithPredicate_False()
        {
            var m = (Maybe<int>)5;
            m.Is(i => i == 6).Should().BeFalse();
        }

        [Test]
        public void WithPredicate_NoValue()
        {
            var m = default(Maybe<int>);
            m.Is(i => true).Should().BeFalse();
        }
    }

    public class EqualsOverride
    {
        [Test]
        public void WithValue_True()
        {
            var m = (Maybe<int>)5;
            m.Equals(5).Should().BeTrue();
        }

        [Test]
        public void WithValue_False()
        {
            var m = (Maybe<int>)5;
            m.Equals(6).Should().BeFalse();
        }

        [Test]
        public void NoValue()
        {
            var m = default(Maybe<int>);
            m.Equals(5).Should().BeFalse();
        }
    }

    public class Flatten
    {
        [Test]
        public void ReturnsInnerMaybe()
        {
            var inner = (Maybe<int>)42;
            var outer = (Maybe<Maybe<int>>)inner;
            var flat = MaybeExtensions.Flatten(outer);
            flat.HasValue.Should().BeTrue();
            flat.GetValueOrDefault(-1).Should().Be(42);
        }

        [Test]
        public void ReturnsDefault_WhenNoValue()
        {
            var outer = default(Maybe<Maybe<int>>);
            var flat = MaybeExtensions.Flatten(outer);
            flat.HasValue.Should().BeFalse();
        }
    }
}
