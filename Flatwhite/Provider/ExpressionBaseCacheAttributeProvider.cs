using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Cache attribute provider to return different cache attribute by different method expression
    /// </summary>
    public class ExpressionBaseCacheAttributeProvider<T, TCacheAttribute> : ICacheAttributeProvider
        where T : class
        where TCacheAttribute : OutputCacheAttribute, new()
    {
        /// <summary>
        /// Initialize an ExpressionBaseCacheAttributeProvider
        /// </summary>
        public ExpressionBaseCacheAttributeProvider()
        {
            Expressions = new List<ExpressionSetting<T, TCacheAttribute>>();
        }
        /// <summary>
        /// Get cache attribute
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.OutputCacheAttributeCache.ContainsKey(methodInfo))
            {
                foreach (var e in Expressions)
                {
                    var m = ExpressionHelper.ToMethodInfo(e.Expression);
                    if (m == methodInfo)
                    {
                        Global.Cache.OutputCacheAttributeCache[methodInfo] = e.CacheAttribute;
                        break;
                    }
                }
                if (!Global.Cache.OutputCacheAttributeCache.ContainsKey(methodInfo))
                {
                    Global.Cache.OutputCacheAttributeCache[methodInfo] = OutputCacheAttribute.Default; 
                }
            }

            return Global.Cache.OutputCacheAttributeCache[methodInfo];
        }

        /// <summary>
        /// List of configured expressions
        /// </summary>
        public List<ExpressionSetting<T, TCacheAttribute>> Expressions { get; }
    }
}