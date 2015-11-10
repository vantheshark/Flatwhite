using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite.WebApi
{
    internal class WebApiCacheAttributeProvider : DefaulAttributeProvider, ICacheAttributeProvider
    {
        public Flatwhite.OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            var attribute = (OutputCacheAttribute) invocationContext[typeof (OutputCacheAttribute).Name];
            return new Flatwhite.OutputCacheAttribute
            {
                Duration = attribute.Duration,
                VaryByCustom = attribute.VaryByCustom,
                VaryByParam = attribute.VaryByParam
            };
        }
    }
}