using System;
using System.Threading;
using Flatwhite.Hot;

namespace Flatwhite
{
    /// <summary>
    /// Bare information about the cache that is useful for <see cref="Phoenix"/> to refresh
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// The store Id whose stores the cache
        /// </summary>
        public int CacheStoreId { get; set; }
        /// <summary>
        /// The cache key
        /// </summary>
        public string CacheKey { get; set; }
        /// <summary>
        /// Cache duration in seconds
        /// </summary>
        public uint CacheDuration { get; set; }
        /// <summary>
        /// Stale while revalidate in seconds
        /// </summary>
        public uint StaleWhileRevalidate { get; set; }
        /// <summary>
        /// Auto refresh
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// Get next refresh time
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetRefreshTime()
        {
            return AutoRefresh && CacheDuration > 0 ? TimeSpan.FromSeconds(CacheDuration) : Timeout.InfiniteTimeSpan;
        }
    }
}