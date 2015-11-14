using System;
using System.Collections.Generic;
using System.Reflection;
using Flatwhite.Provider;
using System.Linq;

namespace Flatwhite.WebApi
{
    internal class WebApiCacheAttributeProvider : DefaulAttributeProvider, ICacheAttributeProvider
    {
        public Flatwhite.OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            var attribute = (OutputCacheAttribute) invocationContext[typeof (OutputCacheAttribute).Name];
            var varyByHeader = (attribute.VaryByHeader ?? "");
            varyByHeader = string.Join(", ", varyByHeader.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => $"headers.{x}"));
            return new Flatwhite.OutputCacheAttribute
            {
                Duration = (int) attribute.MaxAge * 1000,
                VaryByCustom = $"{attribute.VaryByCustom},{varyByHeader}",
                VaryByParam = attribute.VaryByParam,
            };
        }
    }
}