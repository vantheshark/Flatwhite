using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Dependencies;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Build a response from cacheItem but also take into account all cache-request-directive 
    /// This should be resolvable from <see cref="IDependencyResolver" />
    /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
    /// </summary>
    public interface ICacheResponseBuilder
    {
        /// <summary>
        /// Build a <see cref="HttpResponseHeaders" /> from <see cref="CacheControlHeaderValue" /> and <see cref="WebApiCacheItem" />
        /// </summary>
        /// <param name="requestCacheControl"></param>
        /// <param name="cacheItem"></param>
        /// <param name="request"></param>
        /// <returns>Return null if the cacheItem is not suitable for the request cache control, such as max-age, min-fresh is provided and the cache item is not qualified</returns>
        HttpResponseMessage GetResponse(CacheControlHeaderValue requestCacheControl, WebApiCacheItem cacheItem, HttpRequestMessage request);
    }
}