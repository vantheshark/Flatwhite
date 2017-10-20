using Flatwhite.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace Flatwhite
{
    /// <summary>
    /// Global config
    /// </summary>
    public class Global
    {
        // ReSharper disable InconsistentNaming
        internal static readonly string __flatwhite_outputcache_attribute = "__flatwhite_outputcache_attribute";
        internal static readonly string __flatwhite_outputcache_strategy = "__flatwhite_outputcache_strategy";
        internal static readonly string __flatwhite_outputcache_store = "__flatwhite_outputcache_store";
        internal static readonly string __flatwhite_outputcache_key = "__flatwhite_outputcache_key";
        internal static readonly string __flatwhite_outputcache_restored = "__flatwhite_outputcache_restored";
        internal static readonly IDictionary<string, string[]> _revalidationKeys = new ConcurrentDictionary<string, string[]>();
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Internal cache for Flatwhite objects
        /// </summary>
        internal static MethodInfoCache Cache { get; set; }

        /// <summary>
        /// Remember revalidate key to the CacheStore (id = 0)
        /// </summary>
        /// <param name="revalidateKey"></param>
        /// <param name="cacheKey"></param>
        /// <param name="absoluteExpiration"></param>
        public static void RememberRevalidateKey(string revalidateKey, string cacheKey, DateTimeOffset absoluteExpiration)
        {
            string[] keys;
            if (!_revalidationKeys.TryGetValue(revalidateKey, out keys) || !keys.Contains(cacheKey))
            {
                _revalidationKeys[revalidateKey] = new List<string>(keys ?? new string[0]).Union(new[] { cacheKey }).ToArray();
                CacheStoreProvider.GetCacheStore().Set(revalidateKey, _revalidationKeys[revalidateKey], absoluteExpiration);
            }
        }

        /// <summary>
        /// Revalidate the caches with provided revalidateKeys
        /// </summary>
        /// <param name="revalidatedKeys"></param>
        public static void RevalidateCaches(List<string> revalidatedKeys)
        {
            foreach (var key in revalidatedKeys)
            {
                string[] cacheKeys;
                if (!_revalidationKeys.TryGetValue(key, out cacheKeys))
                {
                    cacheKeys = CacheStoreProvider.GetCacheStore().Get(key) as string[];
                }

                if (cacheKeys == null || cacheKeys.Length == 0)
                {
                    Logger.Warn($"Cannot find original cache key(s) for revalidate key '{key}' on host '{Environment.MachineName}'");
                    continue;
                }

                foreach (var cacheKey in cacheKeys)
                {
                    if (Cache.PhoenixFireCage.ContainsKey(cacheKey))
                    {
                        Cache.PhoenixFireCage[cacheKey].Reborn();
                    }
                    else
                    {
                        Logger.Warn($"Cannot find phoenix key '{cacheKey}' on host '{Environment.MachineName}'");
                    }
                }
            }
        }

        /// <summary>
        /// Async notify revalidation events
        /// </summary>
        /// <param name="revalidateKeys"></param>
        /// <returns></returns>
        public static Task RevalidateCachesAsync(List<string> revalidateKeys)
        {
            return Task.Run(() => RevalidateCaches(revalidateKeys));
        }

        static Global()
        {
            Init();
        }

        internal static void Init()
        {
            Cache = new MethodInfoCache();
        
            CacheStrategyProvider = new DefaultCacheStrategyProvider();
            AttributeProvider = new DefaulAttributeProvider();
            HashCodeGeneratorProvider = new DefaultHashCodeGeneratorProvider();
            CacheKeyProvider = new DefaultCacheKeyProvider(HashCodeGeneratorProvider);
            CacheStoreProvider = new DefaultCacheStoreProvider();
            ServiceActivator = new DefaultServiceActivator();
            Logger = new NullLogger();
            BackgroundTaskManager = new DefaultBackgroundTaskManager();

            var configFolder = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            CacheProfileProvider = new YamlCacheProfileProvider(Path.Combine(configFolder ?? "", "cacheProfile.yaml"));
        }
        
        /// <summary>
        /// Cache key provider
        /// </summary>
        public static ICacheKeyProvider CacheKeyProvider { get; set; }

        /// <summary>
        /// Cache strategy provider helps find the suitable cache strategy from the current invocation and context
        /// </summary>
        public static ICacheStrategyProvider CacheStrategyProvider { get; set; }
        
        /// <summary>
        /// Attribute provider
        /// </summary>
        public static IAttributeProvider AttributeProvider { get; set; }
        /// <summary>
        /// Parameter serializer provider
        /// </summary>
        public static IHashCodeGeneratorProvider HashCodeGeneratorProvider { get; set; }

        /// <summary>
        /// A provider to resolve cache stores
        /// </summary>
        public static ICacheStoreProvider CacheStoreProvider { get; set; }

        /// <summary>
        /// The service activator to create instance of service when needed to invoke the MethodInfo for cache refreshing
        /// </summary>
        public static IServiceActivator ServiceActivator { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// Background task manager
        /// </summary>
        internal static IBackgroundTaskManager BackgroundTaskManager { get; set; }

        /// <summary>
        /// Cache profile provider
        /// </summary>
        public static IOutputCacheProfileProvider CacheProfileProvider { get; set; }
    }
}