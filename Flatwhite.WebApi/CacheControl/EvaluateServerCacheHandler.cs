using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Flatwhite.WebApi.CacheControl
{
    /// <summary>
    /// An implementation of <see cref="ICachControlHeaderHandler" /> that use the request uri whether these is an exisisting cache data
    /// </summary>
    public class EvaluateServerCacheHandler : ICachControlHeaderHandler
    {
        private readonly ICacheResponseBuilder _builder;

        /// <summary>
        /// Initializes an object of <see cref="EvaluateServerCacheHandler" /> using a provided <see cref="ICacheResponseBuilder" />
        /// </summary>
        /// <param name="builder"></param>
        public EvaluateServerCacheHandler(ICacheResponseBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            _builder = builder;
        }

        /// <summary>
        /// Try to get the cache from url and httpmethod and build the response if cache is available
        /// </summary>
        /// <param name="cacheControl"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> HandleAsync(CacheControlHeaderValue cacheControl, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldIgnoreCache(cacheControl, request))
            {
                return null;
            }

            var cacheKey = request.GetUrlBaseCacheKey();

            foreach (var cacheStore in Global.CacheStoreProvider.AllAsyncCacheStores)
            {
                var originalKey = await cacheStore.GetAsync(cacheKey).ConfigureAwait(false) as string;
                if (originalKey == null) continue;
                
                var cacheItem = await cacheStore.GetAsync(originalKey).ConfigureAwait(false) as WebApiCacheItem;
                if (cacheItem != null)
                {
                    var response = _builder.GetResponse(cacheControl, cacheItem, request);
                    if (response != null && cacheItem.RequiresPhoenix())
                    {
                        cacheItem.CreatesPhoenixIfNotExist(() => new WebApiPhoenix(cacheItem, request));
                    }
                    return response;
                }
            }
            return null;
        }

        /// <summary>
        /// Determine whether or not should ignore all the cache settings base on the current request and cache control header
        /// </summary>
        /// <param name="cacheControl"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual bool ShouldIgnoreCache(CacheControlHeaderValue cacheControl, HttpRequestMessage request)
        {
            return cacheControl != null && (cacheControl.NoCache || cacheControl.NoStore || cacheControl.MaxAge?.TotalSeconds == 0);
        }
    }
}