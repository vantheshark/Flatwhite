using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Default implementation of <see cref="ICacheAttributeProvider"/> to get first <see cref="OutputCacheAttribute"/>
    /// </summary>
    public class DefaultCacheAttributeProvider : DefaulAttributeProvider, ICacheAttributeProvider
    {
        /// <summary>
        /// Get 1 cache attribute from methodInfo
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.OutputCacheAttributeCache.ContainsKey(methodInfo))
            {
                var att = GetAttributes(methodInfo, invocationContext).FirstOrDefault(a => a is OutputCacheAttribute) ?? OutputCacheAttribute.Default;
                Global.Cache.OutputCacheAttributeCache[methodInfo] = (OutputCacheAttribute) att;
            }

            return Global.Cache.OutputCacheAttributeCache[methodInfo];
        }
    }
}