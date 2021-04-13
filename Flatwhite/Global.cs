﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Global config
    /// </summary>
    public class Global
    {
        internal static readonly string __flatwhite_outputcache_attribute = "__flatwhite_outputcache_attribute";
        internal static readonly string __flatwhite_outputcache_strategy = "__flatwhite_outputcache_strategy";
        internal static readonly string __flatwhite_outputcache_store = "__flatwhite_outputcache_store";
        internal static readonly string __flatwhite_outputcache_key = "__flatwhite_outputcache_key";
        internal static readonly string __flatwhite_outputcache_restored = "__flatwhite_outputcache_restored";
        
        /// <summary>
        /// Internal cache for Flatwhite objects
        /// </summary>
        internal static MethodInfoCache Cache { get; set; }

        /// <summary>
        /// Global router for revalidation event
        /// </summary>
        public static event Action<string> RevalidateEvent;

        /// <summary>
        /// Revalidate the caches with provided revalidateKeys
        /// </summary>
        /// <param name="revalidatedKeys"></param>
        public static void RevalidateCaches(List<string> revalidatedKeys)
        {
            foreach (var key in revalidatedKeys)
            {
                RevalidateEvent?.Invoke(key);
            }
        }

        /// <summary>
        /// Async notify revalidation events
        /// </summary>
        /// <param name="revalidateKeys"></param>
        /// <returns></returns>
        public static Task RevalidateCachesAsync(List<string> revalidateKeys)
        {
            if (RevalidateEvent != null)
            {
                return Task.WhenAll(
                    revalidateKeys.Select(k => Task.Run(() => { RevalidateEvent?.Invoke(k); }))
                );
            }
            return TaskHelpers.DefaultCompleted;
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
        /// Cache strategy provider helps find the suitable cache strategy from the current invocation & context
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