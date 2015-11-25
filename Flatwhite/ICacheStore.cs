using System;

namespace Flatwhite
{
    /// <summary>
    /// Provide methods to save/retrieve cache data
    /// </summary>
    public interface ICacheStore
    {
        /// <summary>
        /// When overridden in a derived class, inserts a cache entry into the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="absoluteExpiration">a value that indicates whether a cache entry should be evicted after a specified duration.</param>
        void Set(string key, object value, DateTimeOffset absoluteExpiration);
        
        /// <summary>
        /// When overridden in a derived class, removes the cache entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.></param>
        /// <returns>An object that represents the value of the removed cache entry that was specified by the key, or null if the specified entry was not found.</returns>
        object Remove(string key);

        /// <summary>
        /// When overridden in a derived class, gets the specified cache entry from the cache as an object.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <returns>The cache entry that is identified by key.</returns>
        object Get(string key);

        /// <summary>
        /// When overridden in a derived class, checks whether the cache entry already exists in the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>true if the cache contains a cache entry with the same key value as key; otherwise, false.</returns>
        bool Contains(string key);

        /// <summary>
        /// The unique number represent the <see cref="ICacheStore" />
        /// </summary>
        int StoreId { get; }
    }
}
