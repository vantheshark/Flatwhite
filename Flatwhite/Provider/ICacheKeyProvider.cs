using System.Collections.Generic;

namespace Flatwhite.Provider
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
        string GetCacheKey(_IInvocation invocation, IDictionary<string, object> invocationContext);

        /// <summary>
        /// Build the revalidation key from provided keyFormat
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="keyFormat"></param>
        /// <returns></returns>
        string GetRevalidateKey(_IInvocation invocation, string keyFormat);
    }
}
