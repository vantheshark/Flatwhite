using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite.Provider
{
    internal class DefaultCacheAttributeProvider : DefaulAttributeProvider, ICacheAttributeProvider
    {

        public OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
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