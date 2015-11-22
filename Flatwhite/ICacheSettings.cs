using System;

namespace Flatwhite
{
    /// <summary>
    /// Provided properties for vary settings which can be used to build the cache key
    /// </summary>
    public interface ICacheSettings
    {
        /// <summary>
        /// Vary by method param
        /// </summary>
        string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the vary-by-custom value which could be used to make the cache key
        /// </summary>
        string VaryByCustom { get; set; }

        /// <summary>
        /// A key to used to delete the cache when an method with relevant <see cref="RevalidateAttribute" /> is invoked
        /// </summary>
        string RevalidationKey { get; set; }

        /// <summary>
        /// The custom cache store type, if provided, the cache store will be resolved by the 
        /// </summary>
        Type CacheStoreType { get; set; }

        /// <summary>
        /// The store id that we want to keep the cache (mem/redis, etc)
        /// Negative value mean this setting is not being used
        /// </summary>
        int CacheStoreId { get; set; }
    }
}