using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Get cache attribute for a method info
    /// </summary>
    public interface ICacheAttributeProvider
    {
        /// <summary>
        /// Get 1 cache attribute
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext);
    }
}