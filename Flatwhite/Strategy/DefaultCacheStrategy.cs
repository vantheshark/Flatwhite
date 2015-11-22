using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Flatwhite.Provider;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// Default strategy which will enable cache for methods with <see cref="OutputCacheAttribute"/> decorated
    /// </summary>
    public class DefaultCacheStrategy : ICacheStrategy
    {
        private readonly IServiceActivator _activator;

        /// <summary>
        /// Initialize CacheStrategy with service activator
        /// </summary>
        /// <param name="activator"></param>
        public DefaultCacheStrategy(IServiceActivator activator = null)
        {
            _activator = activator ?? Global.ServiceActivator;
        }

        /// <summary>
        /// Determine whether it can intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual bool CanIntercept(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.InterceptableCache.ContainsKey(invocation.Method))
            {
                //https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isvirtual(v=vs.110).aspx
                var possible = invocation.Method.ReturnType != typeof (void) && invocation.Method.IsVirtual && !invocation.Method.IsFinal;
                if (possible)
                {
                    var atts = Global.AttributeProvider.GetAttributes(invocation.Method, invocationContext);
                    possible = !atts.Any(a => a is NoInterceptAttribute);
                }
                Global.Cache.InterceptableCache[invocation.Method] = possible;
            }

            return Global.Cache.InterceptableCache[invocation.Method];
        }

        /// <summary>
        /// Get <see cref="ICacheStore" /> for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual ICacheStore GetCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = invocationContext[Global.__flatwhite_outputcache_attribute] as ICacheSettings ?? OutputCacheAttribute.Default;
            ICacheStore cacheStore = null;
            try
            {
                if (att.CacheStoreId > 0)
                {
                    cacheStore = Global.CacheStoreProvider.GetCacheStore(att.CacheStoreId);
                }

                if (cacheStore == null && att.CacheStoreType != null && typeof (ICacheStore).IsAssignableFrom(att.CacheStoreType))
                {
                    cacheStore = Global.CacheStoreProvider.GetCacheStore(att.CacheStoreType);
                }
            }
            catch (KeyNotFoundException)
            {
            }
            return cacheStore ?? Global.CacheStoreProvider.GetCacheStore();
        }

        /// <summary>
        /// Get <see cref="IAsyncCacheStore" /> for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public IAsyncCacheStore GetAsyncCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = invocationContext[Global.__flatwhite_outputcache_attribute] as ICacheSettings ?? OutputCacheAttribute.Default;
            if (att.CacheStoreId > 0)
            {

                var asyncCacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(att.CacheStoreId);
                if (asyncCacheStore != null) return asyncCacheStore;
            }

            if (att.CacheStoreType != null && typeof(IAsyncCacheStore).IsAssignableFrom(att.CacheStoreType))
            {
                try
                {
                    return _activator.CreateInstance(att.CacheStoreType) as IAsyncCacheStore ?? Global.CacheStoreProvider.GetAsyncCacheStore(att.CacheStoreType);
                }
                catch (KeyNotFoundException)
                {
                }
            }

            if (att.CacheStoreType != null && typeof(ICacheStore).IsAssignableFrom(att.CacheStoreType))
            {
                try
                {
                    var syncCacheStore = _activator.CreateInstance(att.CacheStoreType) as ICacheStore ?? Global.CacheStoreProvider.GetCacheStore(att.CacheStoreType);
                    if (syncCacheStore != null) return new CacheStoreAdaptor(syncCacheStore);
                }
                catch (KeyNotFoundException)
                {
                }
            }
            return Global.CacheStoreProvider.GetAsyncCacheStore();
        }

        /// <summary>
        /// Get empty list change monitor
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<IChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var info = invocationContext[Global.__flatwhite_outputcache_attribute] as ICacheSettings;
            if (string.IsNullOrWhiteSpace(info?.RevalidationKey))
            {
                yield break;
            }
            yield return new FlatwhiteCacheEntryChangeMonitor(info.RevalidationKey);
        }
        
        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider => Global.CacheKeyProvider;
    }
}