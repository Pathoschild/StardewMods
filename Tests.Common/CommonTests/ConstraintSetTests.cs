using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="ConstraintSet{T}"/>.</summary>
    [TestFixture]
    class ConstraintSetTests
    {
        /*********
        ** Unit tests
        *********/
        /****
        ** Constructor
        ****/
        /// <summary>Ensure that the <see cref="ConstraintSet{T}"/> constructor sets the expected values.</summary>
        /// <param name="_">A value which is only used to specify the generic type.</param>
        [TestCase(0)]
        [TestCase(0u)]
        [TestCase(0f)]
        [TestCase("")]
        public void Constructor<T>(T _)
        {
            this.CreateAndAssertSet<T>();
        }

        /****
        ** AddBound
        ****/
        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(T)"/> sets the expected values when given one value.</summary>
        /// <param name="value">The value to add.</param>
        [TestCase(1)]
        [TestCase(2u)]
        [TestCase(3f)]
        [TestCase("boop")]
        public void AddBound_SingleValue<T>(T value)
        {
            // arrange
            ConstraintSet<T> set = this.CreateAndAssertSet<T>();

            // act
            bool added = set.AddBound(value);
            bool reAdded = set.AddBound(value);

            // assert
            this.AssertBounds(set, new[] { value }, new T[0]);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(T)"/> sets the expected values when given one case-insensitive string value.</summary>
        [TestCase]
        public void AddBound_SingleValue_WithCaseInsensitiveComparer()
        {
            // arrange
            const string value = "boop";
            ConstraintSet<string> set = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);

            // act
            bool added = set.AddBound(value);
            bool reAdded = set.AddBound(value.ToUpper());

            // assert
            this.AssertBounds(set, new[] { value }, new string[0]);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(T)"/> sets the expected values when given one case-sensitive string value.</summary>
        [TestCase]
        public void AddBound_SingleValue_WithCaseSensitiveComparer()
        {
            // arrange
            const string value = "boop";
            ConstraintSet<string> set = this.CreateAndAssertSet<string>();

            // act
            bool added = set.AddBound(value);
            bool reAdded = set.AddBound(value.ToUpper());

            // assert
            this.AssertBounds(set, new[] { value, value.ToUpper() }, new string[0]);
            added.Should().BeTrue();
            reAdded.Should().BeTrue();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(IEnumerable{T})"/> sets the expected values when given multiple values.</summary>
        /// <param name="values">The values to add.</param>
        [TestCase(1, 2, 3, 4)]
        public void AddBound_MultipleValues(params int[] values)
        {
            // arrange
            ConstraintSet<int> set = this.CreateAndAssertSet<int>();

            // act
            bool added = set.AddBound(values);
            bool reAdded = set.AddBound(values);

            // assert
            this.AssertBounds(set, values, new int[0]);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(IEnumerable{T})"/> sets the expected values when given multiple case-insensitive values.</summary>
        /// <param name="input">The values to add.</param>
        /// <param name="expected">The expected resulting values.</param>
        [TestCase(new[] { "boop", "foo" }, new[] { "boop", "foo" })]
        [TestCase(new[] { "boop", "BOOp", "bOOp", "foo" }, new[] { "boop", "foo" })]
        public void AddBound_MultipleValues_WithCaseInsensitiveComparer(string[] input, string[] expected)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);

            // act
            bool added = set.AddBound(input);
            bool reAdded = set.AddBound(input);

            // assert
            this.AssertBounds(set, expected, new string[0]);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.AddBound(IEnumerable{T})"/> sets the expected values when given multiple case-sensitive values.</summary>
        /// <param name="input">The values to add.</param>
        /// <param name="expected">The expected resulting values.</param>
        [TestCase(new[] { "boop", "foo" }, new[] { "boop", "foo" })]
        [TestCase(new[] { "boop", "BOOp", "bOOp", "foo" }, new[] { "boop", "BOOp", "bOOp", "foo" })]
        public void AddBound_MultipleValues_WithCaseSensitiveComparer(string[] input, string[] expected)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet<string>();

            // act
            bool added = set.AddBound(input);
            bool reAdded = set.AddBound(input);

            // assert
            this.AssertBounds(set, expected, new string[0]);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /****
        ** Exclude
        ****/
        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(T)"/> sets the expected values when given one value.</summary>
        /// <param name="value">The value to add.</param>
        [TestCase(1)]
        [TestCase(2u)]
        [TestCase(3f)]
        [TestCase("boop")]
        public void Exclude_SingleValue<T>(T value)
        {
            // arrange
            ConstraintSet<T> set = this.CreateAndAssertSet<T>();

            // act
            bool added = set.Exclude(value);
            bool reAdded = set.Exclude(value);

            // assert
            this.AssertBounds(set, new T[0], new[] { value });
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(T)"/> sets the expected values when given one case-insensitive string value.</summary>
        [TestCase]
        public void Exclude_SingleValue_WithCaseInsensitiveComparer()
        {
            // arrange
            const string value = "boop";
            ConstraintSet<string> set = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);

            // act
            bool added = set.Exclude(value);
            bool reAdded = set.Exclude(value.ToUpper());

            // assert
            this.AssertBounds(set, new string[0], new[] { value });
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(T)"/> sets the expected values when given one case-sensitive string value.</summary>
        [TestCase]
        public void Exclude_SingleValue_WithCaseSensitiveComparer()
        {
            // arrange
            const string value = "boop";
            ConstraintSet<string> set = this.CreateAndAssertSet<string>();

            // act
            bool added = set.Exclude(value);
            bool reAdded = set.Exclude(value.ToUpper());

            // assert
            this.AssertBounds(set, new string[0], new[] { value, value.ToUpper() });
            added.Should().BeTrue();
            reAdded.Should().BeTrue();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(IEnumerable{T})"/> sets the expected values when given multiple values.</summary>
        /// <param name="values">The values to add.</param>
        [TestCase(1, 2, 3, 4)]
        public void Exclude_MultipleValues(params int[] values)
        {
            // arrange
            ConstraintSet<int> set = this.CreateAndAssertSet<int>();

            // act
            bool added = set.Exclude(values);
            bool reAdded = set.Exclude(values);

            // assert
            this.AssertBounds(set, new int[0], values);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(IEnumerable{T})"/> sets the expected values when given multiple case-insensitive values.</summary>
        /// <param name="input">The values to add.</param>
        /// <param name="expected">The expected resulting values.</param>
        [TestCase(new[] { "boop", "foo" }, new[] { "boop", "foo" })]
        [TestCase(new[] { "boop", "BOOp", "bOOp", "foo" }, new[] { "boop", "foo" })]
        public void Exclude_MultipleValues_WithCaseInsensitiveComparer(string[] input, string[] expected)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);

            // act
            bool added = set.Exclude(input);
            bool reAdded = set.Exclude(input);

            // assert
            this.AssertBounds(set, new string[0], expected);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Exclude(IEnumerable{T})"/> sets the expected values when given multiple case-sensitive values.</summary>
        /// <param name="restrict">The values to add.</param>
        /// <param name="exclude">The expected resulting values.</param>
        [TestCase(new[] { "boop", "foo" }, new[] { "boop", "foo" })]
        [TestCase(new[] { "boop", "BOOp", "bOOp", "foo" }, new[] { "boop", "BOOp", "bOOp", "foo" })]
        public void Exclude_MultipleValues_WithCaseSensitiveComparer(string[] restrict, string[] exclude)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet<string>();

            // act
            bool added = set.Exclude(restrict);
            bool reAdded = set.Exclude(restrict);

            // assert
            this.AssertBounds(set, new string[0], exclude);
            added.Should().BeTrue();
            reAdded.Should().BeFalse();
        }

        /****
        ** Allows
        ****/
        /// <summary>Ensure that <see cref="ConstraintSet{T}.Allows"/> returns the expected values given float values. (NUnit's generic support is finicky, so float stands in for other number types.)</summary>
        /// <param name="restrict">The values to restrict.</param>
        /// <param name="exclude">The values to exclude.</param>
        /// <param name="test">The values to check.</param>
        /// <param name="expectedResult">The result that should be returned for each test value.</param>
        [TestCase(new[] { 1f, 2f, 3f }, new[] { 3f, 4f }, new[] { 1f, 2f }, true)]
        [TestCase(new[] { 1f, 2f, 3f }, new[] { 3f, 4f }, new[] { 3f, 4f, 5f }, false)]
        public void Allows_WithFloats(float[] restrict, float[] exclude, float[] test, bool expectedResult)
        {
            // arrange
            ConstraintSet<float> set = this.CreateAndAssertSet<float>();
            set.AddBound(restrict);
            set.Exclude(exclude);
            this.AssertBounds(set, restrict, exclude);

            // act/assert
            foreach (float value in test)
                set.Allows(value).Should().Be(expectedResult, $"expected '{value}' to {(expectedResult ? "be allowed" : "be excluded")}");
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Allows(T)"/> returns the expected value when given multiple case-sensitive values.</summary>
        /// <param name="restrict">The values to restrict.</param>
        /// <param name="exclude">The values to exclude.</param>
        /// <param name="test">The values to check.</param>
        /// <param name="expectedResult">The result that should be returned for each test value.</param>
        [TestCase(new[] { "boop", "BOOP", "foo" }, new[] { "BOOP" }, new[] { "boop", "foo" }, true)]
        [TestCase(new[] { "boop", "BOOP", "foo" }, new[] { "BOOP" }, new[] { "BOOP", "bOOp", null, "" }, false)]
        public void Allows_WithCaseSensitiveComparer(string[] restrict, string[] exclude, string[] test, bool expectedResult)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet<string>();
            set.AddBound(restrict);
            set.Exclude(exclude);
            this.AssertBounds(set, restrict, exclude);

            // act/assert
            foreach (string value in test)
                set.Allows(value).Should().Be(expectedResult, $"expected '{value}' to {(expectedResult ? "be allowed" : "be excluded")}");
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Allows(T)"/> returns the expected value when given multiple case-insensitive values.</summary>
        /// <param name="restrict">The values to restrict.</param>
        /// <param name="exclude">The values to exclude.</param>
        /// <param name="test">The values to check.</param>
        /// <param name="expectedResult">The result that should be returned for each test value.</param>
        [TestCase(new[] { "boop", "BAR", "foo" }, new[] { "BOOP" }, new[] { "bar", "bAr", "foo" }, true)]
        [TestCase(new[] { "boop", "BOOP", "foo" }, new[] { "BOOP" }, new[] { "boop", "bOOp", "BOOP", null, "" }, false)]
        public void Allows_WithCaseInsensitiveComparer(string[] restrict, string[] exclude, string[] test, bool expectedResult)
        {
            // arrange
            ConstraintSet<string> set = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            set.AddBound(restrict);
            set.Exclude(exclude);
            this.AssertBounds(
                set,
                new HashSet<string>(restrict, StringComparer.OrdinalIgnoreCase).ToArray(),
                new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase).ToArray()
            );

            // act/assert
            foreach (string value in test)
                set.Allows(value).Should().Be(expectedResult, $"expected '{value}' to {(expectedResult ? "be allowed" : "be excluded")}");
        }

        /****
        ** Intersects
        ****/
        /// <summary>Ensure that <see cref="ConstraintSet{T}.Intersects"/> returns the expected value in a bounded intersecting case.</summary>
        [TestCase]
        public void Intersects_Bounded_Intersection()
        {
            // arrange
            ConstraintSet<string> left = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            ConstraintSet<string> right = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            left.AddBound(new[] { "A", "B", "C", "D" });
            right.AddBound(new[] { "C", "D" });
            left.Exclude("C");
            right.Exclude("C");

            // act
            bool leftIntersects = left.Intersects(right);
            bool rightIntersects = right.Intersects(left);

            // assert
            leftIntersects.Should().BeTrue();
            rightIntersects.Should().BeTrue();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Intersects"/> returns the expected value in a bounded non-intersecting case.</summary>
        [TestCase]
        public void Intersects_Bounded_NonIntersection()
        {
            // arrange
            ConstraintSet<string> left = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            ConstraintSet<string> right = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            left.AddBound(new[] { "A", "B", "C" });
            right.AddBound(new[] { "C" });
            left.Exclude("C");
            right.Exclude("C");

            // act
            bool leftIntersects = left.Intersects(right);
            bool rightIntersects = right.Intersects(left);

            // assert
            leftIntersects.Should().BeFalse();
            rightIntersects.Should().BeFalse();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Intersects"/> returns the expected value when both sets are infinite.</summary>
        [TestCase]
        public void Intersects_BothInfinite_Intersection()
        {
            // arrange
            ConstraintSet<string> left = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            ConstraintSet<string> right = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            left.Exclude("C");
            right.Exclude("D");

            // act
            bool leftIntersects = left.Intersects(right);
            bool rightIntersects = right.Intersects(left);

            // assert
            leftIntersects.Should().BeTrue();
            rightIntersects.Should().BeTrue();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Intersects"/> returns the expected value when one set is infinite and they intersect on the bounded sets values.</summary>
        [TestCase]
        public void Intersects_OneInfinite_Intersection()
        {
            // arrange
            ConstraintSet<string> left = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            ConstraintSet<string> right = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            left.Exclude("C");
            right.AddBound(new[] { "A", "B", "C" });

            // act
            bool leftIntersects = left.Intersects(right);
            bool rightIntersects = right.Intersects(left);

            // assert
            leftIntersects.Should().BeTrue();
            rightIntersects.Should().BeTrue();
        }

        /// <summary>Ensure that <see cref="ConstraintSet{T}.Intersects"/> returns the expected value when one set is infinite and they don't intersect on the bounded sets values.</summary>
        [TestCase]
        public void Intersects_OneInfinite_NonIntersection()
        {
            // arrange
            ConstraintSet<string> left = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            ConstraintSet<string> right = this.CreateAndAssertSet(StringComparer.OrdinalIgnoreCase);
            left.Exclude(new[] { "A", "B", "C" });
            right.AddBound(new[] { "A", "B", "C" });

            // act
            bool leftIntersects = left.Intersects(right);
            bool rightIntersects = right.Intersects(left);

            // assert
            leftIntersects.Should().BeFalse();
            rightIntersects.Should().BeFalse();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Create a constraint set and assert that its values match the expected values.</summary>
        /// <typeparam name="T">The set type.</typeparam>
        /// <param name="comparer">The equality comparer to use.</param>
        private ConstraintSet<T> CreateAndAssertSet<T>(IEqualityComparer<T> comparer = null)
        {
            // act
            ConstraintSet<T> set = new ConstraintSet<T>(comparer ?? EqualityComparer<T>.Default);

            // assert
            set.IsInfinite.Should().BeTrue("the set should be infinite immediately after construction because no bounds have been set");
            set.IsBounded.Should().BeFalse("the set should be unbounded immediately after construction because no bounds have been set");
            set.ExcludeValues.Should().BeEmpty("the set should has no exclusions immediately after construction because none were added");
            set.RestrictToValues.Should().BeEmpty("the set should has no restrictions immediately after construction because none were added");

            return set;
        }

        /// <summary>Assert that the boundary values match the expected values.</summary>
        /// <typeparam name="T">The set type.</typeparam>
        /// <param name="set">The set to validate.</param>
        /// <param name="restrictions">The expected restriction values.</param>
        /// <param name="exclusions">The expected exclusion values.</param>
        private void AssertBounds<T>(ConstraintSet<T> set, T[] restrictions, T[] exclusions)
        {
            if (restrictions.Any())
            {
                set.IsInfinite.Should().BeFalse("bound restrictions were added");
                set.IsBounded.Should().BeTrue("bound restrictions ere added");
            }
            else
            {
                set.IsInfinite.Should().BeTrue("no bound restriction was added");
                set.IsBounded.Should().BeFalse("no bound restriction was added");
            }

            set.RestrictToValues.Should().BeEquivalentTo(restrictions);
            set.ExcludeValues.Should().BeEquivalentTo(exclusions);
        }
    }
}
