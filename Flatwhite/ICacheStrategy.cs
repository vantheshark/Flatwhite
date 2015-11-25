using System.Collections.Generic;
using System.ComponentModel;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Provide the cache strategy
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// Determine whether it can cache the invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        bool CanCache(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Get <see cref="ICacheStore" /> for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        ICacheStore GetCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Get <see cref="IAsyncCacheStore" /> for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        IAsyncCacheStore GetAsyncCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Get the change monitor by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        IEnumerable<IChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ICacheKeyProvider CacheKeyProvider { get; }
    }
}