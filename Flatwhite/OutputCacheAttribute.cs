using System;
using System.Runtime.Caching;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method which has return type and you want the library to cache the result
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class OutputCacheAttribute : MethodFilterAttribute, ICacheSettings
    {
        /// <summary>
        /// Default OutputCacheAttribute
        /// </summary>
        internal static readonly OutputCacheAttribute Default = new OutputCacheAttribute();
        private ICacheStrategy _cacheStrategy;

        /// <summary>
        /// Default constructor for OutputCacheAttribute
        /// </summary>
        public OutputCacheAttribute()
        {
            CacheStoreId = -1;
        }

        #region -- Cache params --
        /// <summary>
        /// Gets or sets the cache duration, in miliseconds.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// If set with a positive number the system will try to refresh the cache automatically after <see cref="Duration" /> ms.
        /// The stale cache item will still be return to the caller.
        /// 
        /// The value to set to this field is idealy the amount of time the actual call make the return the fresh value.
        /// </summary>
        public int StaleWhileRevalidate { get; set; }

        /// <summary>
        /// A semicolon-separated list of strings that correspond to to parameter values
        /// </summary>
        public string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the vary-by-custom value which could be used to make the cache key
        /// </summary>
        public string VaryByCustom { get; set; }

        /// <summary>
        /// The custom cache store type, if provided, the cache store will be resolved by the 
        /// </summary>
        public Type CacheStoreType { get; set; }

        /// <summary>
        /// The store id that we want to keep the cache (mem/redis, etc),
        /// Default -1 which means the custom store id is not used
        /// </summary>
        public int CacheStoreId { get; set; }

        /// <summary>
        /// A key to used to delete the cache when an method with relevant <see cref="RevalidateAttribute" /> is invoked
        /// </summary>
        public string RevalidationKey { get; set; }

        #endregion

        /// <summary>
        /// Set the custom cache strategy <see cref="ICacheStrategy" />
        /// </summary>
        /// <param name="cacheStrategy"></param>
        internal void SetCacheStrategy(ICacheStrategy cacheStrategy)
        {
            _cacheStrategy = cacheStrategy;
        }

        /// <summary>
        /// Check to see if the cache is available
        /// </summary>
        /// <param name="methodExecutingContext"></param>
        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
            var strategy = _cacheStrategy ?? Global.CacheStrategyProvider.GetStrategy(methodExecutingContext.Invocation, methodExecutingContext.InvocationContext);
            if (Duration <= 0 || strategy == null || !strategy.CanCache(methodExecutingContext.Invocation, methodExecutingContext.InvocationContext))
            {
                return;
            }
            methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_attribute] = this;
            
            var key = strategy.CacheKeyProvider.GetCacheKey(methodExecutingContext.Invocation, methodExecutingContext.InvocationContext);
            var cacheStore = strategy.GetCacheStore(methodExecutingContext.Invocation, methodExecutingContext.InvocationContext);

            var result = cacheStore.Get(key);
            if (result != null)
            {
                methodExecutingContext.Result = result;
                methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_restored] = true;
                return;
            }
            Global.Logger.Info($"Cache is not available for key {key}");
            methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_store] = cacheStore;
            methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_key] = key;
            methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_strategy] = strategy;
        }

        /// <summary>
        /// Save the return data from invocation to cache
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            if (Duration > 0 && !methodExecutedContext.TryGet<bool>(Global.__flatwhite_outputcache_restored))
            {
                var key = methodExecutedContext.TryGet<string>(Global.__flatwhite_outputcache_key);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                var cacheStore = methodExecutedContext.TryGet<ICacheStore>(Global.__flatwhite_outputcache_store);
                var strategy = methodExecutedContext.TryGet<ICacheStrategy>(Global.__flatwhite_outputcache_strategy);
                var phoenix = CreatePhoenix(methodExecutedContext.Invocation, cacheStore.StoreId, key);

                var changeMonitors = strategy.GetChangeMonitors(methodExecutedContext.Invocation, methodExecutedContext.InvocationContext);
                foreach (var mon in changeMonitors)
                {
                    mon.CacheMonitorChanged += x =>
                    {
                        phoenix.RebornOrDieForever(this);
                    };
                }

                cacheStore.Set(key, methodExecutedContext.Invocation.ReturnValue, DateTime.Now.AddMilliseconds(Duration + StaleWhileRevalidate));
                Global.Cache.Phoenix[key] = phoenix;
            }
        }

        /// <summary>
        /// Create the phoenix object which can refresh the cache itself if StaleWhileRevalidate > 0
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheStoreId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual Phoenix CreatePhoenix(_IInvocation invocation, int cacheStoreId, string key)
        {
            return new Phoenix(invocation, cacheStoreId, key, Duration, StaleWhileRevalidate);
        }
    }
}