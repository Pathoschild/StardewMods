using System;

namespace Pathoschild.Stardew.Common.Utilities
{
    /// <summary>Maintains a cached value which is updated automatically when the cache key changes.</summary>
    /// <typeparam name="TValue">The cached value type.</typeparam>
    internal class Cached<TValue>
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the current cache key.</summary>
        private readonly Func<string> GetCacheKey;

        /// <summary>Fetch the latest value for the cache.</summary>
        private readonly Func<TValue> FetchNew;

        /// <summary>The last cache key which was cached.</summary>
        private string LastCacheKey;

        /// <summary>The cached value.</summary>
        private TValue LastValue;


        /*********
        ** Accessors
        *********/
        /// <summary>Get the cached value, creating it if needed.</summary>
        public TValue Value
        {
            get
            {
                string cacheKey = this.GetCacheKey();
                if (cacheKey != this.LastCacheKey)
                {
                    this.LastCacheKey = cacheKey;
                    this.LastValue = this.FetchNew();
                }

                return this.LastValue;
            }
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getCacheKey">Get the current cache key.</param>
        /// <param name="fetchNew">Fetch the latest value for the cache.</param>
        public Cached(Func<string> getCacheKey, Func<TValue> fetchNew)
        {
            this.GetCacheKey = getCacheKey;
            this.FetchNew = fetchNew;
        }
    }
}
