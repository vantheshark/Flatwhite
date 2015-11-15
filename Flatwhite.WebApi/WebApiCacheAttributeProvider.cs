using System;
using System.Collections.Generic;
using System.Reflection;
using Flatwhite.Provider;
using System.Linq;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Implementation of <see cref="ICacheAttributeProvider"/> for WebApi
    /// </summary>
    public class WebApiCacheAttributeProvider : DefaultCacheAttributeProvider
    {
        /// <summary>
        /// Copy settings from <see cref="OutputCacheAttribute"/> attribute to <see cref="Flatwhite.OutputCacheAttribute"/>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public override Flatwhite.OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!invocationContext.ContainsKey(WebApiExtensions.__webApi))
            {
                return base.GetCacheAttribute(methodInfo, invocationContext);
            }

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