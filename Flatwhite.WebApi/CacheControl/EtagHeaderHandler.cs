using System;
using System.Linq;
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
                // Etag format: fw-StoreId-HashedKey-Checksum
                var requestEtags = request.Headers.IfNoneMatch.Where(t => t.Tag != null && t.Tag.StartsWith("\"fw-")).ToList();
                if (requestEtags.Count > 0)
                {
                    foreach (var etag in requestEtags)
                    {
                        var etagString = etag.Tag.Trim('"');
                        var index = etagString.IndexOf("-", 4, StringComparison.Ordinal);
                        var storeIdString = index > 0 ? etagString.Substring(3, index - 3) : "0";
                        index = etagString.LastIndexOf("-", StringComparison.Ordinal);

                        int storeId;
                        IAsyncCacheStore cacheStore = null;
                        if (int.TryParse(storeIdString, out storeId))
                        {
                            cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(storeId) ?? Global.CacheStoreProvider.GetAsyncCacheStore();
                        }

                        if (cacheStore != null && index > 0)
                        {
                            var hashedKey = etagString.Substring(0, index);
                            var checkSum = etagString.Substring(index + 1);
                            var cacheItem = (await cacheStore.GetAsync(hashedKey)) as WebApiCacheItem;
                            
                            if (cacheItem != null)
                            {
                                if (cacheItem.Checksum == checkSum)
                                {
                                    request.Properties[WebApiExtensions.__webApi_etag_matched] = true;
                                }

                                var response = _builder.GetResponse(cacheControl, cacheItem, request);
                                if (response != null && cacheItem.RequiresPhoenix())
                                {
                                    cacheItem.CreatesPhoenixIfNotExist(() => new WebApiPhoenix(cacheItem, request));
                                }
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