using System;
using Flatwhite.Hot;

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
        /// Gets or sets the cache duration, in seconds.
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// <para>This should be used with <see cref="Duration" /> to indicates that caches MAY serve the cached result in which it appears after it becomes stale, up to the indicated number of seconds</para>
        /// <para>The first call comes to the service and gets a stale cache result will also make the cache system auto refresh once. So if the method is not called
        /// many times in a short period, it's better to turn on <see cref="AutoRefresh" /> to make the cache refresh as soon as it starts to be stale</para> 
        /// </summary>
        public uint StaleWhileRevalidate { get; set; }

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

        /// <summary>
        /// If set to true, the cache will be auto refreshed every <see cref="Duration"/> second(s).
        /// <para>It's a trade-off to turn this on as you don't want too many Timers trying to refresh your cache data very small amout of seconds especially when you have <see cref="Duration"/> too small
        /// and there is so many variaties of the cache (because of ,<see cref="VaryByParam" />). 
        /// </para>
        /// <para>If the method is called many time in a short period with small value of <see cref="Duration"/> setting, it's better to keep this off and use <see cref="StaleWhileRevalidate"/> instead</para>
        /// <para>If the method is not called quickly but you want to keep the cache always available, turn this on and specify the <see cref="StaleWhileRevalidate"/> with a value greater than 0</para>
        /// </summary>
        public bool AutoRefresh { get; set; }

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

            var cacheItem = cacheStore.Get(key) as CacheItem;
            if (cacheItem != null)
            {
                methodExecutingContext.Result = cacheItem.Data;
                methodExecutingContext.InvocationContext[Global.__flatwhite_outputcache_restored] = true;

                if (cacheItem.IsStale())
                {
                    if (!Global.Cache.PhoenixFireCage.ContainsKey(key))
                    {
                        //If this is the first request but the "cacheItem" (Possibly distributed cache item" has the cache
                        CreatePhoenix(methodExecutingContext.Invocation, cacheItem);
                    }
                    if (!AutoRefresh)
                    {
                        RefreshCache(key);
                    }
                }

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
                var cacheItem = new CacheItem
                {
                    Key = key,
                    Data = methodExecutedContext.Result,
                    StoreId = cacheStore.StoreId,
                    StaleWhileRevalidate = StaleWhileRevalidate,
                    MaxAge = Duration,
                    CreatedTime = DateTime.UtcNow
                };
                CreatePhoenix(methodExecutedContext.Invocation, cacheItem);

                var changeMonitors = strategy.GetChangeMonitors(methodExecutedContext.Invocation, methodExecutedContext.InvocationContext);
                foreach (var mon in changeMonitors)
                {
                    mon.CacheMonitorChanged += x =>
                    {
                        RefreshCache(key);
                    };
                }
                
                cacheStore.Set(key, cacheItem, DateTime.UtcNow.AddSeconds(Duration + StaleWhileRevalidate));
            }
        }

        private void RefreshCache(string storedKey)
        {
            if (Global.Cache.PhoenixFireCage.ContainsKey(storedKey))
            {
                Global.Cache.PhoenixFireCage[storedKey].Reborn();
            }
        }

        /// <summary>
        /// Create the phoenix object which can refresh the cache itself if StaleWhileRevalidate > 0
        /// and store by key in Global.Cache.Phoenix
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheItem"></param>
        /// <returns></returns>
        private void CreatePhoenix(_IInvocation invocation, CacheItem cacheItem)
        {
            var cacheInfo = new CacheInfo
            {
                CacheKey = cacheItem.Key,
                CacheStoreId = cacheItem.StoreId,
                CacheDuration = Duration,
                StaleWhileRevalidate = StaleWhileRevalidate,
                AutoRefresh = AutoRefresh
            };
            
            if (Global.Cache.PhoenixFireCage.ContainsKey(cacheItem.Key))
            {
                Global.Cache.PhoenixFireCage[cacheItem.Key].Dispose();
            }

            Global.Cache.PhoenixFireCage[cacheItem.Key] = new Phoenix(invocation, cacheInfo); ;
        }

        /// <summary>
        /// Get all vary by custom string
        /// </summary>
        /// <returns></returns>
        public virtual string GetAllVaryCustomKey()
        {
            return VaryByCustom ?? "";
        }
    }
}