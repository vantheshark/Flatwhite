using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Flatwhite.WebApi.CacheControl
{
    /// <summary>
    /// An implementation of <see cref="ICachControlHeaderHandler" /> that use the etag to check whether these is an exisisting cache data
    /// </summary>
    public class EtagHeaderHandler : ICachControlHeaderHandler
    {
        private readonly ICacheResponseBuilder _builder;

        /// <summary>
        /// Initializes an object of <see cref="EtagHeaderHandler" /> using a provided <see cref="ICacheResponseBuilder" />
        /// </summary>
        /// <param name="builder"></param>
        public EtagHeaderHandler(ICacheResponseBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _builder = builder;
        }

        /// <summary>
        /// Try to get the cache from etag and build the response if cache is available
        /// </summary>
        /// <param name="cacheControl"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> HandleAsync(CacheControlHeaderValue cacheControl, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.IfNoneMatch != null)
            {
                // Etag format: fw-StoreId-Guid
                var requestEtags = request.Headers.IfNoneMatch.Where(t => t.Tag != null && t.Tag.StartsWith("\"fw-")).ToList();
                //request.Headers.CacheControl.
                if (requestEtags.Count > 0)
                {
                    foreach (var etag in requestEtags)
                    {
                        var hashedKey = etag.Tag.Trim('"');
                        var index = hashedKey.IndexOf("-", 4, StringComparison.Ordinal);
                        var storeIdString = index > 0 ? hashedKey.Substring(3, index - 4) : "0";
                        uint storeId;
                        IAsyncCacheStore cacheStore = null;
                        if (uint.TryParse(storeIdString, out storeId))
                        {
                            try
                            {
                                cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(storeId);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        if (cacheStore == null)
                        {
                            cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore();
                        }
                        
                        if (cacheStore != null)
                        {
                            var cacheItem = (await cacheStore.GetAsync(hashedKey)) as CacheItem;

                            if (cacheItem != null)
                            {
                                request.Properties[WebApiExtensions.__webApi_etag_matched] = true;
                                return _builder.GetResponse(cacheControl, cacheItem, request);
                            }
                            if (cacheControl != null && cacheControl.OnlyIfCached)
                            {
                                var response = new HttpResponseMessage {StatusCode = HttpStatusCode.GatewayTimeout};
                                response.Headers.Add("X-Flatwhite-Message", "no cache available");
                                return response;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}