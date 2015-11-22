using System;

namespace Flatwhite
{
    /// <summary>
    /// An event to notify the cache store when the data for the cached item has been changed
    /// </summary>
    /// <param name="state"></param>
    public delegate void CacheMonitorChangeEvent(object state);
    /// <summary>
    /// Provide methods to notify 
    /// </summary>
    public interface IChangeMonitor : IDisposable
    {
        /// <summary>
        /// Cache monitor change event
        /// </summary>
        event CacheMonitorChangeEvent CacheMonitorChanged;
        /// <summary>
        /// Call this method when you want to notify the cache store there is changes regarding the relevant cache item
        /// </summary>
        /// <param name="state"></param>
        void OnChanged(object state);
    }
}