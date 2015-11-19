using System;
using System.Collections.ObjectModel;
using System.Runtime.Caching;

namespace Flatwhite
{
    /// <summary>
    /// An ChangeMonitor that response to GlobalRevalidateEvent and call OnChanged to reset the cache
    /// </summary>
    public class FlatwhiteCacheEntryChangeMonitor : CacheEntryChangeMonitor
    {
        /// <summary>
        /// Initializes an instance of FlatwhiteCacheEntryChangeMonitor with revalidationKey
        /// </summary>
        /// <param name="revalidationKey"></param>
        public FlatwhiteCacheEntryChangeMonitor(string revalidationKey)
        {
            CacheKeys = new ReadOnlyCollection<string>(new [] { revalidationKey });
            UniqueId = Guid.NewGuid().ToString("N");
            LastModified = DateTimeOffset.UtcNow;
            RegionName = "";

            Global.RevalidateEvent += GlobalRevalidateEvent;
            InitializationComplete();
        }

        private void GlobalRevalidateEvent(string revalidationKey)
        {
            if (CacheKeys.Contains(revalidationKey))
            {
                this.OnChanged(revalidationKey);
            }
        }

        /// <summary>
        /// When dispose, unregistered from Global.RevalidateEvent
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Global.RevalidateEvent -= GlobalRevalidateEvent;
        }

        /// <summary>
        /// Gets a value that represents the <see cref="T:System.Runtime.Caching.ChangeMonitor"/> class instance.
        /// </summary>
        /// <returns>
        /// The identifier for a change-monitor instance.
        /// </returns>
        public override string UniqueId { get; }

        /// <summary>
        /// Gets a collection of cache keys that are monitored for changes. 
        /// </summary>
        /// <returns>
        /// A collection of cache keys.
        /// </returns>
        public override ReadOnlyCollection<string> CacheKeys { get; }

        /// <summary>
        /// Gets a value that indicates the latest time (in UTC time) that the monitored cache entry was changed.
        /// </summary>
        /// <returns>
        /// The elapsed time.
        /// </returns>
        public override DateTimeOffset LastModified { get; }

        /// <summary>
        /// Gets the name of a region of the cache.
        /// </summary>
        /// <returns>
        /// The name of a region in the cache. 
        /// </returns>
        public override string RegionName { get; }
    }
}