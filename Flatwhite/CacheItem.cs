using System;

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
        /// The id of the <see cref="ICacheStore" /> where the cache item will be stored
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Return the age of the CacheItem
        /// </summary>
        public uint Age => (uint)Math.Round(DateTime.UtcNow.Subtract(CreatedTime).TotalSeconds);
    }
}
