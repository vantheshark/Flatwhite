using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Global config
    /// </summary>
    public class Global
    {
        /// <summary>
        /// Global router for revalidation event
        /// </summary>
        public static event Action<string> RevalidateEvent;

        /// <summary>
        /// Revalidate the caches with provided revalidateKeys
        /// </summary>
        /// <param name="revalidateKeys"></param>
        public static void RevalidateCaches(List<string> revalidateKeys)
        {
            if (RevalidateEvent != null)
            {
                revalidateKeys.ToList().ForEach(k => RevalidateEvent.BeginInvoke(k, ExecAsyncCallback, null));
            }
        }
        private static void ExecAsyncCallback(IAsyncResult result)
        {
            var asyncResult = result as AsyncResult;
            if (asyncResult == null) return;

            var d = asyncResult.AsyncDelegate as Action<string>;
            d?.EndInvoke(result);
        }

        static Global()
        {
            Cache = new MethodInfoCache();

            ContextProvider = new EmptyContextProvider();
            CacheStrategyProvider = new DefaultCacheStrategyProvider();
            AttributeProvider = new DefaulAttributeProvider();
            CacheAttributeProvider = new DefaultCacheAttributeProvider();
            
            HashCodeGeneratorProvider = new DefaultHashCodeGeneratorProvider();
            HashCodeGeneratorProvider.Register<object>(new DefaultHashCodeGenerator());

            CacheKeyProvider = new DefaultCacheKeyProvider(CacheAttributeProvider, HashCodeGeneratorProvider);

            CacheStoreProvider = new DefaultCacheStoreProvider();
            CacheStoreProvider.RegisterStore(new ObjectCacheStore());
        }

        /// <summary>
        /// Context provider
        /// </summary>
        public static IContextProvider ContextProvider { get; set; }
        /// <summary>
        /// Cache key provider
        /// </summary>
        public static ICacheKeyProvider CacheKeyProvider { get; set; }
        /// <summary>
        /// Cache strategy provider
        /// </summary>
        public static ICacheStrategyProvider CacheStrategyProvider { get; set; }
        /// <summary>
        /// OutputCache attribute provider
        /// </summary>
        public static ICacheAttributeProvider CacheAttributeProvider { get; set; }
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
        /// Internal cache for Flatwhite objects
        /// </summary>
        internal static MethodInfoCache Cache { get; set; }
    }
}