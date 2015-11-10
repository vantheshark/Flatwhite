using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;


namespace Flatwhite
{
    /// <summary>
    /// Provide the cache strategy
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// Determine whether it can intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        bool CanIntercept(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Get cache time by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        int GetCacheTime(_IInvocation invocation, IDictionary<string, object> invocationContext);
        
        /// <summary>
        /// Get the change monitor by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        IEnumerable<ChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ICacheKeyProvider CacheKeyProvider { get; }
    }
}