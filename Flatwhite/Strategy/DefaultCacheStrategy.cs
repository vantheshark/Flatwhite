using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using Flatwhite.Provider;


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
        public bool CanIntercept(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.InterceptableCache.ContainsKey(invocation.Method))
            {
                //https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isvirtual(v=vs.110).aspx
                var possible = invocation.Method.ReturnType != typeof (void) && invocation.Method.IsVirtual && !invocation.Method.IsFinal;
                if (possible)
                {
                    var atts = _attributeProvider.GetAttributes(invocation.Method, invocationContext);
                    possible = !atts.Any(a => a is NoInterceptAttribute);
                }
                Global.Cache.InterceptableCache[invocation.Method] = possible;
            }

            return Global.Cache.InterceptableCache[invocation.Method];
        }

        /// <summary>
        /// Get cache time from <see cref="OutputCacheAttribute"/>
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual int GetCacheTime(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            OutputCacheAttribute att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            return att?.Duration ?? 0;
        }

        /// <summary>
        /// Get cache store id for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns>The id (number) of the cache store which is going to store the cache item</returns>
        public uint GetCacheStoreId(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            OutputCacheAttribute att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            return att?.CacheStoreId ?? 0;
        }

        /// <summary>
        /// Get empty list change monitor
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            OutputCacheAttribute att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            if (string.IsNullOrWhiteSpace(att?.RevalidationKey))
            {
                yield break;
            }
            yield return new FlatwhiteCacheEntryChangeMonitor(att.RevalidationKey);
        }

        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider => Global.CacheKeyProvider;
    }
}