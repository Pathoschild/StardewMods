using Common.Utilities;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="HumanSortComparerTests"/>.</summary>
    [TestFixture]
    class HumanSortComparerTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Ensure that the <see cref="ConstraintSet{T}"/> constructor sets the expected values.</summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="expectedResult">The expected sort order for <paramref name="a"/> relative to <paramref name="b"/>.</param>

        // null is always less than another value
        [TestCase(null, null, 0)]
        [TestCase("", null, 1)]
        [TestCase("   ", null, 1)]

        // alphabetical sort
        [TestCase("A", "Z", -1)]
        [TestCase("  A", "Z", -1)]

        // numeric sort
        [TestCase("1", "2", -1)]
        [TestCase("1", "2", -1)]
        [TestCase("2", "10", -1)]

        // numeric sort with preceding zeros
        [TestCase("009", "2", -1)]
        [TestCase("001", "2", -1)]
        [TestCase("010", "10", -1)]

        // mixed sequences
        [TestCase("ABC1_", "ABC10_", -1)]
        [TestCase("ABC1_", "ABC2_", -1)]
        [TestCase("ABC2_", "ABC10_", -1)]
        [TestCase("ABC002_", "ABC1_", -1)]

        // long numeric sequences
        [TestCase("AB9223372036854775807", "AB9223372036854775806", 1)] // long.MaxValue, long.MaxValue - 1
        [TestCase("AB10000000000000000000", "AB5", -1)] // values higher than long.MaxValue are sorted alphabetically
        public void Compare(string a, string b, int expectedResult)
        {
            // arrange
            var comparer = new HumanSortComparer();

            // act
            int result = comparer.Compare(a, b);
            int oppositeResult = comparer.Compare(b, a);

            // assert
            Assert.That(result == expectedResult, $"Expected sort order to be {expectedResult}, but got {result} instead.");
            Assert.That(oppositeResult == -expectedResult, $"Expected the inverted sort order to be {-expectedResult}, but got {oppositeResult} instead.");
        }
    }
}
