namespace DataRelated.Tests;

public class SpecificationTests
{
    [Test]
    public void ExpressionSpecification_Apply_FiltersQueryable()
    {
        int[] source = { 1, 2, 3, 4, 5, 6 };
        ISpecification<int> spec = Specifications.From<int>(x => x > 3);

        IQueryable<int> query = source.AsQueryable();
        var result = spec.Apply(query).ToArray();

        result.Should().Equal(new[] { 4, 5, 6 });
    }

    [Test]
    public void ExpressionSpecification_IsSatisfiedBy_Works()
    {
        ISpecification<int> spec = Specifications.From<int>(x => x % 2 == 0);

        spec.IsSatisfiedBy(2).Should().BeTrue();
        spec.IsSatisfiedBy(3).Should().BeFalse();
    }

    [Test]
    public void FailureSpecification_Instance_ReturnsEmptyAndFalse()
    {
        ISpecification<int> spec = Specifications.Fail<int>();

        int[] data = { 1, 2, 3 };
        var result = spec.Apply(data.AsQueryable());

        result.Any().Should().BeFalse();
        spec.IsSatisfiedBy(1).Should().BeFalse();
    }

    [Test]
    public void PermissiveSpecification_Instance_ReturnsAllAndTrue()
    {
        ISpecification<int> spec = Specifications.PermitAll<int>();

        int[] data = { 7, 8 };
        var result = spec.Apply(data.AsQueryable());

        result.SequenceEqual(data).Should().BeTrue();
        spec.IsSatisfiedBy(42).Should().BeTrue();
    }

    [Test]
    public void Combine_NullOrEmpty_ReturnsPermissive()
    {
        var fromNull = CombinedSpecification<int>.Create(null!);
        fromNull.Should().BeSameAs(PermissiveSpecification<int>.Instance);

        var fromEmpty = CombinedSpecification<int>.Create(Array.Empty<ISpecification<int>>());
        fromEmpty.Should().BeSameAs(PermissiveSpecification<int>.Instance);
    }

    [Test]
    public void Combine_SingleSpec_ReturnsThatSpec()
    {
        ISpecification<int> single = Specifications.From<int>(x => x > 0);
        ISpecification<int>[] arr = { single };

        var result = CombinedSpecification<int>.Create(arr);

        result.Should().BeSameAs(single);
    }

    [Test]
    public void Combine_WhenContainsFailure_ReturnsFailureInstance()
    {
        ISpecification<int> s1 = Specifications.From<int>(x => x > 0);
        ISpecification<int> fail = FailureSpecification<int>.Instance;
        ISpecification<int> s2 = Specifications.From<int>(x => x < 100);

        var result = CombinedSpecification<int>.Create(new[] { s1, fail, s2 });

        result.Should().BeSameAs(fail);
    }

    [Test]
    public void Combine_SkipsPermissive_AndReturnsSingleWhenOnlyOneRemains()
    {
        ISpecification<int> permissive = PermissiveSpecification<int>.Instance;
        ISpecification<int> spec = Specifications.From<int>(x => x == 5);

        var result = CombinedSpecification<int>.Create(new[] { permissive, spec });

        result.Should().BeSameAs(spec);
    }

    [Test]
    public void Combine_FlattensNestedCombinedSpecifications()
    {
        ISpecification<int> a = Specifications.From<int>(x => x > 0);
        ISpecification<int> b = Specifications.From<int>(x => x % 2 == 0);
        ISpecification<int> c = Specifications.From<int>(x => x < 100);

        var inner = Specifications.Combine(a, b); // CombinedSpecification
        var outer = CombinedSpecification<int>.Create(new ISpecification<int>[] { inner, c });

        outer.Should().BeOfType<CombinedSpecification<int>>();
        var combined = (CombinedSpecification<int>)outer;
        combined.Specifications.Should().Contain(new[] { a, b, c });
        combined.Specifications.Count.Should().Be(3);
    }

    [Test]
    public void Combine_MultipleSpecifications_CreateCombinedAndApplyWorks()
    {
        ISpecification<int> positive = Specifications.From<int>(x => x > 0);
        ISpecification<int> even = Specifications.From<int>(x => x % 2 == 0);

        var combined = Specifications.Combine(positive, even);
        combined.Should().BeOfType<CombinedSpecification<int>>();

        int[] data = { -2, -1, 0, 1, 2, 3, 4 };
        var applied = combined.Apply(data.AsQueryable()).ToArray();

        // only positive even numbers expected
        applied.Should().Equal(new[] { 2, 4 });

        combined.IsSatisfiedBy(2).Should().BeTrue();
        combined.IsSatisfiedBy(3).Should().BeFalse();
    }

    [Test]
    public void Specifications_Helpers_Work()
    {
        var fromExpr = Specifications.From<int>(x => x == 1);
        fromExpr.IsSatisfiedBy(1).Should().BeTrue();

        var fail = Specifications.Fail<int>();
        fail.IsSatisfiedBy(1).Should().BeFalse();

        var permit = Specifications.PermitAll<int>();
        permit.IsSatisfiedBy(1).Should().BeTrue();

        var combined = Specifications.Combine(fromExpr, permit);
        // fromExpr and permit -> permit is ignored so single remains
        combined.Should().BeSameAs(fromExpr);
    }
}
