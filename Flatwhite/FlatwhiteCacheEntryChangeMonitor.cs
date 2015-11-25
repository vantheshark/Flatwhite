using System.Collections.ObjectModel;

namespace Flatwhite
{
    /// <summary>
    /// An ChangeMonitor that response to GlobalRevalidateEvent and call OnChanged to reset the cache
    /// </summary>
    public class FlatwhiteCacheEntryChangeMonitor : IChangeMonitor
    {
        private bool _disposing;
        /// <summary>
        /// Initializes an instance of FlatwhiteCacheEntryChangeMonitor with revalidationKey
        /// </summary>
        /// <param name="revalidationKey"></param>
        public FlatwhiteCacheEntryChangeMonitor(string revalidationKey = null)
        {
            if (!string.IsNullOrWhiteSpace(revalidationKey))
            {
                CacheKeys = new ReadOnlyCollection<string>(new[] {revalidationKey});
            }

            Global.RevalidateEvent += GlobalRevalidateEvent;
        }

        private void GlobalRevalidateEvent(string revalidationKey)
        {
            if (CacheKeys != null && CacheKeys.Contains(revalidationKey) && !_disposing)
            {
                OnChanged(revalidationKey);
            }
        }

        /// <summary>
        /// When dispose, unregistered from Global.RevalidateEvent
        /// </summary>
        public void Dispose()
        {
            if (!_disposing)
            {
                Global.RevalidateEvent -= GlobalRevalidateEvent;
            }
            _disposing = true;
        }
       
        /// <summary>
        /// Gets a collection of cache keys that are monitored for changes. 
        /// </summary>
        /// <returns>
        /// A collection of cache keys.
        /// </returns>
        public ReadOnlyCollection<string> CacheKeys { get; }

        /// <summary>
        /// Cache monitor change event
        /// </summary>
        public event CacheMonitorChangeEvent CacheMonitorChanged;

        /// <summary>
        /// Call this method when you want to notify the cache store there is changes regarding the relevant cache item
        /// </summary>
        /// <param name="state"></param>
        public void OnChanged(object state)
        {
            if (CacheMonitorChanged != null && !_disposing)
            {
                CacheMonitorChanged(state);
            }
        }
    }
}