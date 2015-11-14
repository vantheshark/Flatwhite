using System.ComponentModel;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Global config
    /// </summary>
    public class Global
    {
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