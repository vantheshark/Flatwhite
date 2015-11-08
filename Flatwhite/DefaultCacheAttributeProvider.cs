using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite
{
    internal class DefaultCacheAttributeProvider : ICacheAttributeProvider
    {
        private readonly IDictionary<MethodInfo, OutputCacheAttribute> _methodInfoCache = new Dictionary<MethodInfo, OutputCacheAttribute>();

        public OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!_methodInfoCache.ContainsKey(methodInfo))
            {
                var att = methodInfo.GetCustomAttributes(typeof(OutputCacheAttribute), true).LastOrDefault() as OutputCacheAttribute;

                if (att == null && methodInfo.DeclaringType != null)
                {
                    att = methodInfo.DeclaringType.GetCustomAttributes(typeof(OutputCacheAttribute), true).LastOrDefault() as OutputCacheAttribute;
                }
                _methodInfoCache[methodInfo] = att ?? OutputCacheAttribute.Default;
            }

            return _methodInfoCache[methodInfo];
        }
    }
}