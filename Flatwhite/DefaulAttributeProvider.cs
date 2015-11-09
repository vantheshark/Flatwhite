using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Default attribute provider
    /// </summary>
    public class DefaulAttributeProvider : IAttributeProvider
    {
        private readonly IDictionary<MethodInfo, List<Attribute>> _methodInfoCache = new Dictionary<MethodInfo, List<Attribute>>();

        /// <summary>
        /// Get all attributes decorated on method or declarative type
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public IEnumerable<Attribute> GetAttributes(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!_methodInfoCache.ContainsKey(methodInfo))
            {
                var atts = methodInfo.GetCustomAttributes(typeof (Attribute), true).OfType<Attribute>().ToList();

                if (methodInfo.DeclaringType != null)
                {
                    atts.AddRange(methodInfo.DeclaringType.GetCustomAttributes(typeof (Attribute), true).OfType<Attribute>());
                }
                _methodInfoCache[methodInfo] = atts;
            }

            return _methodInfoCache[methodInfo];
        }
        
    }
}