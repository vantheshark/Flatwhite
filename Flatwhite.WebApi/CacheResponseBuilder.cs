using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Build a response from cacheItem but also take into account all cache-request-directive 
    /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
    /// </summary>
    public class CacheResponseBuilder : ICacheResponseBuilder
    {
        /// <summary>
        /// Provide a single method to try to build a <see cref="HttpResponseHeaders" /> from <see cref="CacheControlHeaderValue" />  and <see cref="HttpRequestMessage" />
        /// </summary>
        public virtual HttpResponseMessage GetResponse(CacheControlHeaderValue cacheControl, CacheItem cacheItem, HttpRequestMessage request)
        {
            var age = Math.Round(DateTime.UtcNow.Subtract(cacheItem.CreatedTime).TotalSeconds);
            var responseCacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(cacheItem.MaxAge),
            };
            var response = request.CreateResponse();
            bool stale = cacheControl?.MaxAge != null && cacheControl.MaxAge.Value.TotalSeconds > 0 && cacheControl.MaxAge.Value.TotalSeconds < age;

            if (cacheItem.MaxAge < age)
            {
                if (cacheItem.StaleWhileRevalidate > 0 &&
                    cacheControl != null &&
                    cacheControl.MaxStale &&
                    cacheControl.MaxStaleLimit.HasValue &&
                    cacheControl.MaxStaleLimit.Value.TotalSeconds > (age - cacheItem.MaxAge))
                {
                    //  https://tools.ietf.org/html/rfc5861
                    responseCacheControl.Extensions.Add(new NameValueHeaderValue("stale-while-revalidate", cacheItem.StaleWhileRevalidate.ToString()));
                }
                stale = true;
            }
            if (stale)
            {
                response.Headers.Add("X-Flatwhite-Warning", "Response is Stale");
                //https://tools.ietf.org/html/rfc7234#page-31
                response.Headers.Add("Warning", $"110 - \"Response is Stale\"");
            }

            response.StatusCode = HttpStatusCode.NotModified;
            response.Headers.Age = TimeSpan.FromSeconds(age);
            response.Headers.CacheControl = responseCacheControl;
            return response;
        }
    }
}