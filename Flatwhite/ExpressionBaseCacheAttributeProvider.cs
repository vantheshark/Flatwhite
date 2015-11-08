using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// Cache attribute provider to return different cache attribute by different method expression
    /// </summary>
    public class ExpressionBaseCacheAttributeProvider<T, TCacheAttribute> : ICacheAttributeProvider
        where T : class
        where TCacheAttribute : OutputCacheAttribute, new()
    {
        private readonly IDictionary<MethodInfo, OutputCacheAttribute> _cache = new Dictionary<MethodInfo, OutputCacheAttribute>();
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
            if (!_cache.ContainsKey(methodInfo))
            {
                foreach (var e in Expressions)
                {
                    var m = ExpressionHelper.ToMethodInfo(e.Expression);
                    if (m == methodInfo)
                    {
                        _cache[methodInfo] = e.CacheAttribute;
                        break;
                    }
                }
                if (!_cache.ContainsKey(methodInfo))
                {
                    _cache[methodInfo] = OutputCacheAttribute.Default; 
                }
            }

            return _cache[methodInfo];
        }

        /// <summary>
        /// List of configured expressions
        /// </summary>
        public List<ExpressionSetting<T, TCacheAttribute>> Expressions { get; }
    }
}