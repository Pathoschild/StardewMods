using System;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Common.CommonTests
{
    /// <summary>Unit tests for <see cref="Cached{TValue}"/>.</summary>
    [TestFixture]
    class CachedTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Ensure that <see cref="Cached{T}.Value"/> doesn't refetch the value if the cache key didn't change.</summary>
        [TestCase]
        public void SameCacheKey_DoesNotRefetch()
        {
            // arrange
            int timesFetched = 0;
            var cached = new Cached<int>(
                getCacheKey: () => "static",
                fetchNew: () => ++timesFetched
            );

            // act
            _ = cached.Value;
            _ = cached.Value;
            int lastValue = cached.Value;

            // assert
            Assert.AreEqual(1, timesFetched, "The value was fetched more than once.");
            Assert.AreEqual(lastValue, timesFetched, "The cached value doesn't match the expected value.");
        }

        /// <summary>Ensure that <see cref="Cached{T}.Value"/> refetches the value each time the cache key changes.</summary>
        [TestCase]
        public void DifferentCacheKeys_Refetches()
        {
            // arrange
            int cacheKey = 0;
            int timesFetched = 0;
            var cached = new Cached<int>(
                getCacheKey: () => $"{++cacheKey}",
                fetchNew: () => ++timesFetched
            );

            // act
            int valueA = cached.Value;
            int valueB = cached.Value;
            int valueC = cached.Value;

            // assert
            Assert.AreEqual(timesFetched, 3, $"The value should have been fetched three times (once per {cached.Value} call).");
            Assert.AreEqual(valueA, 1, "The first return value should have been 1.");
            Assert.AreEqual(valueB, 2, "The second return value should have been 2.");
            Assert.AreEqual(valueC, 3, "The third return value should have been 3.");
        }
    }
}
