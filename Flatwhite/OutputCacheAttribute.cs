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
        /// Gets or sets a value in miliseconds that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.
        /// </summary>
        /// TODO: public int SlidingExpiration { get; set; }

        /// <summary>
        /// The duration in miliseconds the cache engine should keep the expired cache value while refreshing the cache data asynchronously
        /// </summary>
        /// TODO: public int RefreshingTimeout { get; set; }

        /// <summary>
        /// A semicolon-separated list of strings that correspond to to parameter values
        /// </summary>
        public string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the vary-by-custom value.
        /// </summary>
        public string VaryByCustom { get; set; }
    }
}