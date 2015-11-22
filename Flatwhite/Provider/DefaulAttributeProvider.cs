using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Default attribute provider
    /// </summary>
    public class DefaulAttributeProvider : IAttributeProvider
    {
        /// <summary>
        /// Get all attributes decorated on method or declarative type
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public IEnumerable<Attribute> GetAttributes(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.AttributeCache.ContainsKey(methodInfo))
            {
                var atts = methodInfo.GetCustomAttributes(typeof (Attribute), true).OfType<Attribute>().ToList();

                if (methodInfo.DeclaringType != null)
                {
                    atts.AddRange(methodInfo.DeclaringType.GetCustomAttributes(typeof (Attribute), true).OfType<Attribute>());
                }
                Global.Cache.AttributeCache[methodInfo] = atts;
            }

            return Global.Cache.AttributeCache[methodInfo];
        }
    }
}