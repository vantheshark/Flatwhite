using System;
using System.Threading;

namespace Flatwhite
{
    /// <summary>
    /// A cache item object that keeps some details about the data to be cached
    /// </summary>
    public class CacheItem
    {
        /// <summary>
        /// Cache key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The response data
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The time the cache data is generated
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// Max age for the cache item in seconds
        /// </summary>
        public uint MaxAge { get; set; }

        /// <summary>
        /// If set with a positive number the system will try to refresh the cache after a call to the method automatically after <see cref="MaxAge" /> seconds.
        /// </summary>
        public uint StaleWhileRevalidate { get; set; }

        /// <summary>
        /// Auto refresh
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// The id of the <see cref="ICacheStore" /> where the cache item will be stored
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Return the age of the CacheItem in seconds
        /// </summary>
        public uint Age => (uint)Math.Round(DateTime.UtcNow.Subtract(CreatedTime).TotalSeconds);

        /// <summary>
        /// Return true if the cache item is started to stale
        /// </summary>
        public bool IsStale()
        {
            return DateTime.UtcNow.Subtract(CreatedTime).TotalMilliseconds > MaxAge * 1000;
        }

        /// <summary>
        /// Get next refresh time
        /// </summary>
        /// <returns></returns>
        internal TimeSpan GetRefreshTime()
        {
            return AutoRefresh && MaxAge > 0 ? TimeSpan.FromSeconds(MaxAge) : Timeout.InfiniteTimeSpan;
        }
    }
}
