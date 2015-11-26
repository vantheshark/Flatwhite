using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// A cache strategy to cache for all methods
    /// </summary>
    public class CacheOutputForAllMethod : DefaultCacheStrategy, IDynamicCacheStrategy
    {
        private readonly OutputCacheAttribute _cacheAttribute;

        internal CacheOutputForAllMethod(uint defaultDuration)
        {
            _cacheAttribute = new OutputCacheAttribute() {Duration = defaultDuration};
            _cacheAttribute.SetCacheStrategy(this);
        }

        /// <summary>
        /// Set vary by params
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod VaryByParam(string @params)
        {
            _cacheAttribute.VaryByParam = @params;
            return this;
        }

        /// <summary>
        /// Set vary by custom
        /// </summary>
        /// <param name="customParams"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod VaryByCustom(string customParams)
        {
            _cacheAttribute.VaryByCustom = customParams;
            return this;
        }

        /// <summary>
        /// Set cache duration in seconds
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod Duration(uint durationSeconds)
        {
            _cacheAttribute.Duration = durationSeconds;
            return this;
        }

        /// <summary>
        /// Get all attributes
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<MethodInfo, OutputCacheAttribute>> IDynamicCacheStrategy.GetCacheAttributes()
        {
            yield return new KeyValuePair<MethodInfo, OutputCacheAttribute>(null, _cacheAttribute);
        }
    }
}