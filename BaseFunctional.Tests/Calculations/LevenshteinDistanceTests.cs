using BaseFunctional.Calculations;

namespace BaseFunctional.Tests.Calculations;

public static class LevenshteinDistanceTests
{
    public class CalculateStringDistance
    {
        [Test]
        public void IdenticalStrings_ReturnsZero()
        {
            var s = "abcdef";
            var t = "abcdef";

            var dist = LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default);

            dist.Should().Be(0);
        }

        [Test]
        public void EmptyToNonEmpty_ReturnsLengthOfNonEmpty()
        {
            var s = "";
            var t = "abc";

            LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default)
                .Should().Be(3);
        }

        [Test]
        public void NonEmptyToEmpty_ReturnsLengthOfNonEmpty()
        {
            var t = "";
            var s = "abc";

            LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default)
                .Should().Be(3);
        }

        [Test]
        public void TypicalExample_KittenToSitting_IsThree()
        {
            var s = "kitten";
            var t = "sitting";

            LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default)
                .Should().Be(3);
        }

        [Test]
        public void TypicalExample_SaturdayToSunday_IsThree()
        {
            var s = "saturday";
            var t = "sunday";

            LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default)
                .Should().Be(3);
        }

        [Test]
        public void CaseSensitivity_DefaultComparer_IsCaseSensitive()
        {
            var a = "Hello";
            var b = "hello";

            // With default comparer, difference in case counts as a substitution.
            LevenshteinDistance.CalculateStringDistance(a, b, EqualityComparer<char>.Default)
                .Should().Be(1);
        }

        [Test]
        public void CaseSensitivity_IgnoreCaseComparer_TreatsAsEqual()
        {
            var a = "Hello";
            var b = "hello";

            LevenshteinDistance.CalculateStringDistance(a, b, new IgnoreCaseCharComparer())
                .Should().Be(0);
        }

        [Test]
        public void Transposition_IsNotTreatedAsSingleOperation()
        {
            // Levenshtein (not Damerau) treats transposition as two operations:
            var a = "ab";
            var b = "ba";

            LevenshteinDistance.CalculateStringDistance(a, b, EqualityComparer<char>.Default)
                .Should().Be(2);
        }

        [TestCase("flaw", "lawn")]
        [TestCase("", "x")]
        [TestCase("algorithm", "altruistic")]
        [TestCase("distance", "difference")]
        public void Symmetry_Property_Holds_ForVariousPairs(string s, string t)
        {
            var d1 = LevenshteinDistance.CalculateStringDistance(s, t, EqualityComparer<char>.Default);
            var d2 = LevenshteinDistance.CalculateStringDistance(t, s, EqualityComparer<char>.Default);
            d1.Should().Be(d2, because: $"{s} vs {t} should be symmetric");
        }
    }

    // Helper comparer that ignores character case.
    private sealed class IgnoreCaseCharComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y) => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);

        public int GetHashCode(char obj) => char.ToUpperInvariant(obj).GetHashCode();
    }

    public class CalculateEditSequence
    {
        [Test]
        public void IdenticalSequences_ReturnsEmpty()
        {
            var s = "hello".ToCharArray();
            var t = "hello".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(5);
            ops.All(o => o.Operation == LevenshteinDistance.Operation.Same).Should().BeTrue();
        }

        [Test]
        public void InsertSingleCharacter_ReturnsInsertOperation()
        {
            var s = "".ToCharArray();
            var t = "x".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Should().ContainSingle()
                .Which.Operation.Should().Be(LevenshteinDistance.Operation.Insert);

            // Insert must provide the new value (the target char)
            ops.Should().ContainSingle(o => o.Operation == LevenshteinDistance.Operation.Insert && o.NewValue.Equals('x'));
        }

        [Test]
        public void InsertSeveralCharacters_ReturnsInsertOperations()
        {
            var s = "".ToCharArray();
            var t = "xyz".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(3);
            ops[0].Operation.Should().Be(LevenshteinDistance.Operation.Insert);
            ops[0].NewValue.Should().Be('x');
            ops[1].Operation.Should().Be(LevenshteinDistance.Operation.Insert);
            ops[1].NewValue.Should().Be('y');
            ops[2].Operation.Should().Be(LevenshteinDistance.Operation.Insert);
            ops[2].NewValue.Should().Be('z');
        }

        [Test]
        public void DeleteSingleCharacter_ReturnsDeleteOperation()
        {
            var s = "x".ToCharArray();
            var t = "".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Should().ContainSingle()
                .Which.Operation.Should().Be(LevenshteinDistance.Operation.Delete);

            // Delete must provide the old value (the source char)
            ops.Should().ContainSingle(o => o.Operation == LevenshteinDistance.Operation.Delete && o.OldValue.Equals('x'));
        }

        [Test]
        public void DeleteSeveralCharacters_ReturnsDeleteOperations()
        {
            var s = "xyz".ToCharArray();
            var t = "".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(3);
            ops[0].Operation.Should().Be(LevenshteinDistance.Operation.Delete);
            ops[0].OldValue.Should().Be('x');
            ops[1].Operation.Should().Be(LevenshteinDistance.Operation.Delete);
            ops[1].OldValue.Should().Be('y');
            ops[2].Operation.Should().Be(LevenshteinDistance.Operation.Delete);
            ops[2].OldValue.Should().Be('z');
        }

        [Test]
        public void SubstituteSingleCharacter_ReturnsSubstituteOperation()
        {
            var s = "abc".ToCharArray();
            var t = "abx".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(3);
            ops[0].Operation.Should().Be(LevenshteinDistance.Operation.Same);
            ops[1].Operation.Should().Be(LevenshteinDistance.Operation.Same);
            ops[2].Operation.Should().Be(LevenshteinDistance.Operation.Substitute);
            ops[2].OldValue.Should().Be('c');
            ops[2].NewValue.Should().Be('x');
        }

        [Test]
        public void SubstituteSeveralCharacters_ReturnsSubstituteOperations()
        {
            var s = "abc".ToCharArray();
            var t = "xyz".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(3);
            ops[0].Operation.Should().Be(LevenshteinDistance.Operation.Substitute);
            ops[0].OldValue.Should().Be('a');
            ops[0].NewValue.Should().Be('x');
            ops[1].Operation.Should().Be(LevenshteinDistance.Operation.Substitute);
            ops[1].OldValue.Should().Be('b');
            ops[1].NewValue.Should().Be('y');
            ops[2].Operation.Should().Be(LevenshteinDistance.Operation.Substitute);
            ops[2].OldValue.Should().Be('c');
            ops[2].NewValue.Should().Be('z');
        }

        [Test]
        public void CatToBats_ProducesExpectedSubstituteAndInsert()
        {
            var s = "cat".ToCharArray();
            var t = "bats".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            ops.Count.Should().Be(4);
            ops[0].Operation.Should().Be(LevenshteinDistance.Operation.Substitute);
            ops[0].OldValue.Should().Be('c');
            ops[0].NewValue.Should().Be('b');
            ops[1].Operation.Should().Be(LevenshteinDistance.Operation.Same);
            ops[2].Operation.Should().Be(LevenshteinDistance.Operation.Same);
            ops[3].Operation.Should().Be(LevenshteinDistance.Operation.Insert);
            ops[3].NewValue.Should().Be('s');
        }

        [Test]
        public void MultipleEdits_ProducesCorrectOperations_OrderNotStrictlyAssumed()
        {
            var s = "kitten".ToCharArray();
            var t = "sitting".ToCharArray();

            var ops = LevenshteinDistance.CalculateEditSequence(s, t, EqualityComparer<char>.Default);

            // Known minimal edits between "kitten" and "sitting":
            // substitute 'k' -> 's', substitute 'e' -> 'i', insert 'g'
            ops.Should().Contain(o => o.Operation == LevenshteinDistance.Operation.Substitute && o.OldValue.Equals('k') && o.NewValue.Equals('s'));
            ops.Should().Contain(o => o.Operation == LevenshteinDistance.Operation.Substitute && o.OldValue.Equals('e') && o.NewValue.Equals('i'));
            ops.Should().Contain(o => o.Operation == LevenshteinDistance.Operation.Insert && o.NewValue.Equals('g'));
        }
    }
}
