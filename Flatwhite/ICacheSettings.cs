using System;

namespace Flatwhite
{
    /// <summary>
    /// Provided properties for vary settings which can be used to build the cache key
    /// </summary>
    internal interface ICacheSettings
    {
        /// <summary>
        /// Vary by method param
        /// </summary>
        string VaryByParam { get; set; }

        /// <summary>
        /// A key format to used to delete the cache when an method with relevant <see cref="RevalidateAttribute" /> is invoked
        /// </summary>
        string RevalidateKeyFormat { get; set; }

        /// <summary>
        /// The custom cache store type, if provided, the cache store will be resolved by the 
        /// </summary>
        Type CacheStoreType { get; set; }

        /// <summary>
        /// The store id that we want to keep the cache (mem/redis, etc)
        /// Negative value mean this setting is not being used
        /// </summary>
        int CacheStoreId { get; set; }

        /// <summary>
        /// Get all vary by custom value
        /// </summary>
        /// <returns></returns>
        string GetAllVaryCustomKey();
    }
}