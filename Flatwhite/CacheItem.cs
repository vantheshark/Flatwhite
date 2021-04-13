﻿using System;
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
        /// If set to true, the cache will be auto refreshed every <see cref="MaxAge"/> second(s).
        /// <para>It's a trade-off to turn this on as you don't want too many Timers trying to refresh your cache data very small amout of seconds especially when you have <see cref="MaxAge"/> too small
        /// and there is so many variaties of the cache (because of VaryByParam). 
        /// </para>
        /// <para>If the api endpoint is an busy endpoint with small value of <see cref="MaxAge"/>, it's better to keep this off and use <see cref="StaleWhileRevalidate"/></para>
        /// <para>If the endpoint is not busy but you want to keep the cache always available, turn this on and specify the <see cref="StaleWhileRevalidate"/> with a value greater than 0</para>
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
        /// Return true if the cache item has started to stale
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

        internal virtual CacheItem CloneWithoutData()
        {
            var clone = (CacheItem)MemberwiseClone();
            clone.Data = null;
            return clone;
        }
    }
}
