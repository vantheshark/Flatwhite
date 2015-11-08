using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// Default strategy which will enable cache for methods with <see cref="OutputCacheAttribute"/> decorated
    /// </summary>
    public class DefaultCacheStrategy : ICacheStrategy
    {
        /// <summary>
        /// The cache attribute provider
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected readonly ICacheAttributeProvider _cacheAttributeProvider;

        /// <summary>
        /// Initialize default cache strategy with a <see cref="ICacheAttributeProvider"/>
        /// </summary>
        /// <param name="cacheAttributeProvider"></param>
        public DefaultCacheStrategy(ICacheAttributeProvider cacheAttributeProvider)
        {
            _cacheAttributeProvider = cacheAttributeProvider;
        }
        
        /// <summary>
        /// Get cache time from <see cref="OutputCacheAttribute"/>
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual int GetCacheTime(IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            OutputCacheAttribute att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            return att?.Duration ?? 0;
        }

        /// <summary>
        /// Get empty list change monitor
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ChangeMonitor> GetChangeMonitors(IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            yield break;
        }

        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider => Global.CacheKeyProvider;
    }
}