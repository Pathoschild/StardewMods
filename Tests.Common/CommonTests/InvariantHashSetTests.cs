using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="InvariantHashSet"/>.</summary>
    [TestFixture]
    class InvariantHashSetTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Ensure that Add yields the expected case-insensitive count.</summary>
        [TestCase("a", "b", "c", ExpectedResult = 3)]
        [TestCase("a", "A", "á", ExpectedResult = 2)]
        [TestCase("a", "A", "a", ExpectedResult = 1)]
        public int Add_Count(params string[] values)
        {
            InvariantHashSet set = new InvariantHashSet(values);
            return set.Count;
        }

        /// <summary>Ensure that Contains yields the expected case-insensitive result.</summary>
        [TestCase("d", "a", "b", "c", ExpectedResult = false)]
        [TestCase("a", "a", "b", "c", ExpectedResult = true)]
        [TestCase("A", "a", "b", "c", ExpectedResult = true)]
        [TestCase("a", "A", "b", "c", ExpectedResult = true)]
        [TestCase("á", "a", "b", "c", ExpectedResult = false)]
        public bool Contains(string search, params string[] values)
        {
            InvariantHashSet set = new InvariantHashSet(values);
            return set.Contains(search);
        }
    }
}
