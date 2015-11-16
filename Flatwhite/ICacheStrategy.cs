using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using Flatwhite.Provider;


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
        /// Get cache store id for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        uint GetCacheStoreId(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Get the change monitor by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        IEnumerable<ChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext, string cacheKey);

        /// <summary>
        /// Cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ICacheKeyProvider CacheKeyProvider { get; }
    }
}