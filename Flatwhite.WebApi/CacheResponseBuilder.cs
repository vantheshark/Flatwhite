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
            if (cacheControl != null && cacheControl.OnlyIfCached && cacheItem == null)
            {
                var errorResponse = new HttpResponseMessage {StatusCode = HttpStatusCode.GatewayTimeout};
                errorResponse.Headers.Add("X-Flatwhite-Message", "no cache available");
                return errorResponse;
            }

            if (cacheItem == null)
            {
                return null;
            }

            var age = cacheItem.Age;

            var responseCacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(Math.Max(cacheItem.MaxAge - age, 0)),
            };

            var cacheNotQualified = false;
            bool stale = cacheControl?.MaxAge?.TotalSeconds > 0 && cacheControl.MaxAge.Value.TotalSeconds < age;
            
            if (cacheItem.MaxAge < age)
            {
                stale = true;

                if (cacheItem.StaleWhileRevalidate > 0 &&
                    cacheControl != null &&
                    cacheControl.MaxStale &&
                    cacheControl.MaxStaleLimit.HasValue &&
                    cacheControl.MaxStaleLimit.Value.TotalSeconds > (age - cacheItem.MaxAge))
                {
                    //  https://tools.ietf.org/html/rfc5861
                    responseCacheControl.Extensions.Add(new NameValueHeaderValue("stale-while-revalidate", cacheItem.StaleWhileRevalidate.ToString()));
                }
            }

            var response = request.CreateResponse();
            if (cacheControl?.MinFresh?.TotalSeconds > age)
            {
                response.Headers.Add("X-Flatwhite-Warning", "Cache freshness lifetime not qualified");
                cacheNotQualified = true;
            }

            if ((stale || cacheNotQualified) && !cacheItem.IgnoreRevalidationRequest)
            {
                return null;
            }

            if (stale)
            {
                response.Headers.Add("X-Flatwhite-Warning", "Response is Stale");
                //https://tools.ietf.org/html/rfc7234#page-31
                response.Headers.Add("Warning", $"110 - \"Response is Stale\"");
            }
            
            response.Headers.Age = TimeSpan.FromSeconds(age);
            response.Headers.CacheControl = responseCacheControl;

            if (request.Properties.ContainsKey(WebApiExtensions.__webApi_etag_matched))
            {
                response.StatusCode = HttpStatusCode.NotModified;
            }
            else
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new ByteArrayContent(cacheItem.Content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(cacheItem.ResponseMediaType)
                {
                    CharSet = cacheItem.ResponseCharSet
                };
                response.Headers.ETag = new EntityTagHeaderValue($"\"{cacheItem.Key}-{cacheItem.Checksum}\"");
            }
            return response;
        }
        /*
            cache-request-directive =
           "no-cache"                          ; Section 14.9.1
         | "no-store"                          ; Section 14.9.2
         | "max-age" "=" delta-seconds         ; Section 14.9.3, 14.9.4
         | "max-stale" [ "=" delta-seconds ]   ; Section 14.9.3
         | "min-fresh" "=" delta-seconds       ; Section 14.9.3
         | "no-transform"                      ; Section 14.9.5
         | "only-if-cached"                    ; Section 14.9.4
         | cache-extension                     ; Section 14.9.6    
        */
    }
}