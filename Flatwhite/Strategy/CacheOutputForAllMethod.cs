namespace Flatwhite.Strategy
{
    /// <summary>
    /// A cache strategy to cache for all methods
    /// </summary>
    public class CacheOutputForAllMethod : DefaultCacheStrategy
    {
        private OutputCacheAttribute CacheAttribute => ((SingleCacheAttributeProvider) _cacheAttributeProvider).Attribute;

        internal CacheOutputForAllMethod(int defaultDuration) : base(new SingleCacheAttributeProvider(new OutputCacheAttribute {Duration = defaultDuration }))
        {
            CacheKeyProvider = new DefaultCacheKeyProvider(_cacheAttributeProvider);
        }

        /// <summary>
        /// Set vary by params
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod VaryByParam(string @params)
        {
            CacheAttribute.VaryByParam = @params;
            return this;
        }

        /// <summary>
        /// Set vary by custom
        /// </summary>
        /// <param name="customParams"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod VaryByCustom(string customParams)
        {
            CacheAttribute.VaryByCustom = customParams;
            return this;
        }

        /// <summary>
        /// Set cache duration in miliseconds
        /// </summary>
        /// <param name="durationMiliseconds"></param>
        /// <returns></returns>
        public CacheOutputForAllMethod Duration(int durationMiliseconds)
        {
            CacheAttribute.Duration = durationMiliseconds;
            return this;
        }

        /// <summary>
        /// Cache key provider
        /// </summary>
        public override ICacheKeyProvider CacheKeyProvider { get; }
    }
}