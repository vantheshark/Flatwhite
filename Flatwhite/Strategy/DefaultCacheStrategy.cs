using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// Default strategy which will enable cache for methods with <see cref="OutputCacheAttribute"/> decorated
    /// </summary>
    public class DefaultCacheStrategy : ICacheStrategy
    {
        private readonly IAttributeProvider _attributeProvider;

        /// <summary>
        /// The cache attribute provider
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected readonly ICacheAttributeProvider _cacheAttributeProvider;
        private readonly IDictionary<MethodInfo, bool> _methodInfoCache = new Dictionary<MethodInfo, bool>();

        /// <summary>
        /// Initialize default cache strategy with a <see cref="ICacheAttributeProvider"/>
        /// </summary>
        /// <param name="attributeProvider"></param>
        /// <param name="cacheAttributeProvider"></param>
        public DefaultCacheStrategy(IAttributeProvider attributeProvider, ICacheAttributeProvider cacheAttributeProvider)
        {
            _attributeProvider = attributeProvider;
            _cacheAttributeProvider = cacheAttributeProvider;
        }

        /// <summary>
        /// Determine whether it can intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public bool CanIntercept(IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (!_methodInfoCache.ContainsKey(invocation.Method))
            {
                var possible = invocation.Method.ReturnType != typeof (void) &&
                              (invocation.Method.IsVirtual || invocation.Method.DeclaringType != null && invocation.Method.DeclaringType.IsInterface);
                if (possible)
                {
                    var atts = _attributeProvider.GetAttributes(invocation.Method, invocationContext);
                    possible = !atts.Any(a => a is NoInterceptAttribute);
                }
                _methodInfoCache[invocation.Method] = possible;
            }

            return _methodInfoCache[invocation.Method];
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