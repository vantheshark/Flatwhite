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
        /// Cache attribute provider
        /// </summary>
        public static ICacheAttributeProvider CacheAttributeProvider { get; set; }
    }
}