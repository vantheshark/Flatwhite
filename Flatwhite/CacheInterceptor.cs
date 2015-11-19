using System;
using System.Runtime.Caching;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Use this to intercept services to enable caching on methods that have return value
    /// <para>Only methods decorated with <see cref="OutputCacheAttribute"/> will have its result cached</para>
    /// </summary>
    public class CacheInterceptor : _IInterceptor
    {
        private readonly ICacheStrategy _cacheStrategy;
        private readonly IContextProvider _contextProvider;
        
        
        private readonly object _cacheLock = new object();

        /// <summary>
        /// Specify time to live for caching and optional changeMonitor factory
        /// </summary>
        /// <param name="contextProvider"></param>
        /// <param name="cacheStrategy">If not provided, the strategy will be resolved from Global.CacheStrategyProvider</param>
        public CacheInterceptor(
            IContextProvider contextProvider,
            ICacheStrategy cacheStrategy = null)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException(nameof(contextProvider));
            }
            _contextProvider = contextProvider;
            _cacheStrategy = cacheStrategy;
        }

        /// <summary>
        /// Main method to get value from cache if any, set value to cache if there was no cache
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(_IInvocation invocation)
        {
            var context = _contextProvider.GetContext();
            var strategy = _cacheStrategy ?? Global.CacheStrategyProvider.GetStrategy(invocation, context);
            
            if (!strategy.CanIntercept(invocation, context))
            {
                invocation.Proceed();
                return;
            }

            var cacheTime = strategy.GetCacheTime(invocation, context);
            if (cacheTime <= 0)
            {
                invocation.Proceed();
                return;
            }

            var key = strategy.CacheKeyProvider.GetCacheKey(invocation, context);
            
            lock (_cacheLock)
            {
                var cacheStoreId = strategy.GetCacheStoreId(invocation, context);
                var cacheStore = Global.CacheStoreProvider.GetCacheStore(cacheStoreId);
                var result = cacheStore.Get(key);
                if (result == null) // No cache
                {
                    invocation.Proceed();
                    var policy = new CacheItemPolicy {AbsoluteExpiration = DateTime.UtcNow.AddMilliseconds(cacheTime)};
                    var changeMonitors = strategy.GetChangeMonitors(invocation, context);
                    foreach(var mon in changeMonitors)
                    {
                        policy.ChangeMonitors.Add(mon);
                    }
                    
                    cacheStore.Set(key, invocation.ReturnValue, policy);
                }
                else
                {
                    invocation.ReturnValue = result;
                }
            }
        }
    }
}