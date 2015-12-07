using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Flatwhite.Provider;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// Default strategy which will enable cache for methods with <see cref="OutputCacheAttribute"/> decorated
    /// </summary>
    public class DefaultCacheStrategy : ICacheStrategy
    {
        /// <summary>
        /// Dynamic proxy doesn't work for none virtual or final methods so this is false by default.
        /// However, derive of this class such as WebApiCacheStrategy can ignore this because WebAPI doesn't use dynamic proxy
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanCacheNoneVirtualOrFinalMethods()
        {
            return false;
        }

        /// <summary>
        /// Determine whether it can intercept the invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual bool CanCache(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.InterceptableCache.ContainsKey(invocation.Method))
            {
                var possible = invocation.Method.ReturnType != typeof(void);
                if (!CanCacheNoneVirtualOrFinalMethods())
                {
                    //https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isvirtual(v=vs.110).aspx
                    possible &= invocation.Method.IsVirtual && !invocation.Method.IsFinal;
                }

                if (possible && typeof(Task).IsAssignableFrom(invocation.Method.ReturnType) && invocation.Method.ReturnType.IsGenericType)
                {
                    possible = invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                }

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
            var att = invocationContext.TryGetByKey<ICacheSettings>(Global.__flatwhite_outputcache_attribute, OutputCacheAttribute.Default);
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
        public virtual IAsyncCacheStore GetAsyncCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = invocationContext.TryGetByKey<ICacheSettings>(Global.__flatwhite_outputcache_attribute, OutputCacheAttribute.Default);
            if (att.CacheStoreId > 0)
            {
                try
                {
                    var asyncCacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(att.CacheStoreId);
                    if (asyncCacheStore != null) return asyncCacheStore;
                }
                catch (Exception ex)
                {
                    Global.Logger.Error($"Cannot resolve cache store with id {att.CacheStoreId}", ex);
                }
            }

            if (att.CacheStoreType != null && typeof(IAsyncCacheStore).IsAssignableFrom(att.CacheStoreType))
            {
                try
                {
                    var asyncCacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(att.CacheStoreType);
                    if (asyncCacheStore != null) return asyncCacheStore;
                }
                catch (KeyNotFoundException)
                {
                }
            }

            if (att.CacheStoreType != null && typeof(ICacheStore).IsAssignableFrom(att.CacheStoreType))
            {
                try
                {
                    var syncCacheStore = Global.CacheStoreProvider.GetCacheStore(att.CacheStoreType);
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
            var att = invocationContext.TryGetByKey<ICacheSettings>(Global.__flatwhite_outputcache_attribute, OutputCacheAttribute.Default);
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