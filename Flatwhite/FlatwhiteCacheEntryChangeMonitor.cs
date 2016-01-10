namespace Flatwhite
{
    /// <summary>
    /// An ChangeMonitor that response to GlobalRevalidateEvent and call OnChanged to reset the cache
    /// </summary>
    public class FlatwhiteCacheEntryChangeMonitor : IChangeMonitor
    {
        private readonly string _revalidationKey;
        private bool _disposing;
        /// <summary>
        /// Initializes an instance of FlatwhiteCacheEntryChangeMonitor with revalidationKey
        /// </summary>
        /// <param name="revalidationKey"></param>
        public FlatwhiteCacheEntryChangeMonitor(string revalidationKey)
        {
            if (!string.IsNullOrWhiteSpace(revalidationKey))
            {
                _revalidationKey = revalidationKey;
                Global.RevalidateEvent += GlobalRevalidateEvent;
            }
        }

        private void GlobalRevalidateEvent(string revalidationKey)
        {
            if (_revalidationKey == revalidationKey && !_disposing)
            {
                OnChanged(revalidationKey);
                Dispose();
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