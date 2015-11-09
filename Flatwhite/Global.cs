namespace Flatwhite
{
    /// <summary>
    /// Global config
    /// </summary>
    public class Global
    {
        static Global()
        {
            ContextProvider = new EmptyContextProvider();
            CacheProvider = new ObjectCacheProvider();
            CacheStrategyProvider = new DefaultCacheStrategyProvider();
            AttributeProvider = new DefaulAttributeProvider();
            CacheAttributeProvider = new DefaultCacheAttributeProvider();
            CacheKeyProvider = new DefaultCacheKeyProvider(CacheAttributeProvider);
        }

        /// <summary>
        /// Context provider
        /// </summary>
        public static IContextProvider ContextProvider { get; set; }
        /// <summary>
        /// Cache provider
        /// </summary>
        public static ICacheProvider CacheProvider { get; set; }
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
    }
}