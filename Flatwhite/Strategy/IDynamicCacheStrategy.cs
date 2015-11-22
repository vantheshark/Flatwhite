using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// Provide method(s) to get all dynamic OutputCacheAttribute
    /// </summary>
    public interface IDynamicCacheStrategy : ICacheStrategy
    {
        /// <summary>
        /// Get all attributes
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<MethodInfo, OutputCacheAttribute>> GetCacheAttributes();
    }
}