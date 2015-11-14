using System;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method which has return type and you want the library to cache the result
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class OutputCacheAttribute : Attribute
    {
        /// <summary>
        /// Default OutputCacheAttribute
        /// </summary>
        public static readonly OutputCacheAttribute Default = new OutputCacheAttribute();

        /// <summary>
        /// Gets or sets the cache duration, in miliseconds.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// https://tools.ietf.org/html/rfc5861
        /// </summary>
        /// public uint StaleWhileRevalidate { get; set; }

        /// <summary>
        /// https://tools.ietf.org/html/rfc5861#4.1
        /// </summary>
        ///public uint StaleIfError { get; set; }

        /// <summary>
        /// A semicolon-separated list of strings that correspond to to parameter values
        /// </summary>
        public string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the vary-by-custom value which could be used to make the cache key
        /// </summary>
        public string VaryByCustom { get; set; }

        /// <summary>
        /// The custom cache store type, if provided, the cache store will be resolved by the 
        /// </summary>
        public Type CacheStoreType { get; set; }

        /// <summary>
        /// The store id that we want to keep the cache (mem/redis, etc)
        /// </summary>
        public uint CacheStoreId { get; set; }
    }
}