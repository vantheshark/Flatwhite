using System;
using System.Security.Cryptography;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A cache item object that keeps some details about the data to be cached
    /// </summary>
    public class WebApiCacheItem : CacheItem
    {
        /// <summary>
        /// The response data
        /// </summary>
        public byte[] Content { get; set; }

        private string _checkSum;
        /// <summary>
        /// The checksum of the Content
        /// </summary>
        public string Checksum {
            get
            {
                if (Content == null)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(_checkSum))
                {
                    using (var md5Hash = MD5.Create())
                    {
                        _checkSum = md5Hash.ComputeHash(Content).ToHex();
                    }
                }
                return _checkSum;
            }
            internal set { _checkSum = value; }
        }

        /// <summary>
        /// Media type
        /// </summary>
        public string ResponseMediaType { get; set; }

        /// <summary>
        /// Charset
        /// </summary>
        public string ResponseCharSet { get; set; }

        /// <summary>
        /// https://tools.ietf.org/html/rfc5861#4.1
        /// </summary>
        public uint StaleIfError { get; set; }

        /// <summary>
        /// A cache MAY be configured to return stale responses without validation
        /// <para>If set to TRUE, the server will return cache item as soon as the cache item is available and ignore all cache control directives sent from client
        /// such as no-cache, no-store or max-age, max-stale. Warning 110 (Response is stale) will be included in the response header</para>
        /// <para>This may be helpful to guarantee that the endpoint will not revalidate the cache all the time by some one sending request with no-cache header</para>
        /// </summary>
        public bool IgnoreRevalidationRequest { get; set; }

        /// <summary>
        /// If set to true, the cache will be auto refreshed every <see cref="OutputCacheAttribute.MaxAge"/> second(s).
        /// <para>It's a trade-off to turn this on as you don't want too many Timers trying to refresh your cache data very small amout of seconds especially when you have <see cref="OutputCacheAttribute.MaxAge"/> too small
        /// and there is so many variaties of the cache (because of VaryByParam). 
        /// </para>
        /// <para>If the api endpoint is an busy endpoint with small value of <see cref="OutputCacheAttribute.MaxAge"/>, it's better to keep this off and use <see cref="OutputCacheAttribute.StaleWhileRevalidate"/></para>
        /// <para>If the endpoint is not busy but you want to keep the cache always available, turn this on and specify the <see cref="OutputCacheAttribute.StaleWhileRevalidate"/> with a value greater than 0</para>
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// Clone the WebApiCacheItem without the content and with a fresh CreatedTime
        /// </summary>
        /// <returns></returns>
        internal override CacheItem CloneWithoutData()
        {
            var clone = (WebApiCacheItem) base.CloneWithoutData();
            clone.Checksum = null;
            clone.Content = null;
            return clone;
        }
    }
}
