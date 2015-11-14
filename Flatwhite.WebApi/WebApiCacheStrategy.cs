using Flatwhite.Provider;
using Flatwhite.Strategy;

namespace Flatwhite.WebApi
{
    internal class WebApiCacheStrategy : DefaultCacheStrategy
    {
        internal WebApiCacheStrategy() : base(Global.AttributeProvider, new WebApiCacheAttributeProvider())
        {
            CacheKeyProvider = new DefaultCacheKeyProvider(_cacheAttributeProvider, Global.HashCodeGeneratorProvider);
        }
        
        /// <summary>
        /// Cache key provider
        /// </summary>
        public override ICacheKeyProvider CacheKeyProvider { get; }
    }
}
