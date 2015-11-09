using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite
{
    internal class DefaultCacheAttributeProvider : DefaulAttributeProvider, ICacheAttributeProvider
    {
        private readonly IDictionary<MethodInfo, OutputCacheAttribute> _methodInfoCache = new Dictionary<MethodInfo, OutputCacheAttribute>();

        public OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!_methodInfoCache.ContainsKey(methodInfo))
            {
                var att = GetAttributes(methodInfo, invocationContext).FirstOrDefault(a => a is OutputCacheAttribute) ?? OutputCacheAttribute.Default;
                _methodInfoCache[methodInfo] = (OutputCacheAttribute) att;
            }

            return _methodInfoCache[methodInfo];
        }
    }
}