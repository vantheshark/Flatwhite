using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flatwhite
{
    /// <summary>
    /// Async cache provider
    /// </summary>
    public interface IAsyncCacheStore
    {
        /// <summary>
        /// When overridden in a derived class, inserts a cache entry into the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">An object that contains eviction details for the cache entry. This object provides more options for eviction than a simple absolute expiration.</param>
        Task SetAsync(string key, object value, CacheItemPolicy policy);

        /// <summary>
        /// When overridden in a derived class, removes the cache entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.></param>
        /// <returns>An object that represents the value of the removed cache entry that was specified by the key, or null if the specified entry was not found.</returns>
        Task<object> RemoveAsync(string key);

        /// <summary>
        /// When overridden in a derived class, gets the specified cache entry from the cache as an object.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <returns>The cache entry that is identified by key.</returns>
        Task<object> GetAsync(string key);

        /// <summary>
        /// When overridden in a derived class, checks whether the cache entry already exists in the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>true if the cache contains a cache entry with the same key value as key; otherwise, false.</returns>
        Task<bool> ContainsAsync(string key);

        /// <summary>
        /// The unique number represent the <see cref="IAsyncCacheStore" />
        /// </summary>
        uint StoreId { get; }
    }
}
