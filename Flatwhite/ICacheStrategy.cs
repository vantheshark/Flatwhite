using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace Flatwhite
{
    /// <summary>
    /// Provide the cache strategy
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// Get cache time by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        int GetCacheTime(IInvocation invocation, IDictionary<string, object> invocationContext);
        
        /// <summary>
        /// Get the change monitor by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        IEnumerable<ChangeMonitor> GetChangeMonitors(IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ICacheKeyProvider CacheKeyProvider { get; }
    }
}