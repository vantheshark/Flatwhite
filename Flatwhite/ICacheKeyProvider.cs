using System.Collections.Generic;
using Castle.DynamicProxy;

namespace Flatwhite
{
    /// <summary>
    /// A provider to resolve a unique key for caching by invocation and context
    /// </summary>
    public interface ICacheKeyProvider
    {
        /// <summary>
        /// Get unique cache key
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        string GetCacheKey(IInvocation invocation, IDictionary<string, object> invocationContext);
    }
}
