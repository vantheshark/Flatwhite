using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flatwhite.Hot;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A web api phoenix to support auto refresh for webApi
    /// <para>
    /// It will send a request to the same endpoint in the background with a special header to force Flatwhite to invoke the controller instead of getting the stale cache.
    /// </para>
    /// </summary>
    public class WebApiPhoenix : Phoenix
    {
        private readonly WebApiCacheItem _cacheItem;
        private readonly HttpRequestMessage _clonedRequestMessage;

        /// <summary>
        /// Initializes a WebApiPhoenix
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheItem">This should the the WebApiCacheItem instance</param>
        /// <param name="requestMessage"></param>
        public WebApiPhoenix(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage requestMessage) : base(invocation, cacheItem)
        {
            _cacheItem = cacheItem;
            _clonedRequestMessage = new HttpRequestMessage
            {
                RequestUri = requestMessage.RequestUri,
                Method = requestMessage.Method,
                Version = requestMessage.Version
            };
            if (!string.IsNullOrWhiteSpace(WebApiExtensions._fwConfig.LoopbackAddress))
            {
                _clonedRequestMessage.RequestUri = new Uri($"{WebApiExtensions._fwConfig.LoopbackAddress}{_clonedRequestMessage.RequestUri.PathAndQuery}");
            }
            
            _clonedRequestMessage.Content = null;

            foreach (var h in requestMessage.Headers)
            {
                _clonedRequestMessage.Headers.Add(h.Key, h.Value);
            }

            _clonedRequestMessage.Headers.CacheControl = requestMessage.Headers.CacheControl ?? new CacheControlHeaderValue();
        }
        

        /// <summary>
        /// Send a http request with special header to loop back address to by pass the cache
        /// </summary>
        /// <returns></returns>
        protected override async Task<IPhoenixState> FireAsync()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                using (var client = GetHttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(_cacheItem.MaxAge - _cacheItem.Age + Math.Max(_cacheItem.StaleWhileRevalidate, _cacheItem.StaleIfError));
                    _clonedRequestMessage.Headers.CacheControl.Extensions.Add(new NameValueHeaderValue(WebApiExtensions.__cacheControl_flatwhite_force_refresh, "true"));
                    _clonedRequestMessage.Properties.Clear();
                    var response = await client.SendAsync(_clonedRequestMessage, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                }

                sw.Stop();
                Global.Logger.Info($"{nameof(WebApiPhoenix)} updated key: \"{_cacheItem.Key}\", store: \"{_cacheItem.StoreId}\", request: {_clonedRequestMessage.RequestUri.PathAndQuery}, duration: {sw.ElapsedMilliseconds}ms");
                Retry(_info.GetRefreshTime());
                _phoenixState = new InActivePhoenix();
                return _phoenixState;
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while refreshing key {_info.Key}, store \"{_info.StoreId}\". Will retry after 1 second.", ex);
                Retry(TimeSpan.FromSeconds(1));
                throw;
            }
        }

        /// <summary>
        /// Get HttpClient
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough, ExcludeFromCodeCoverage]
        protected virtual IHttpClient GetHttpClient()
        {
            return new HttpClientAdaptor();
        }
        [ExcludeFromCodeCoverage]
        private class HttpClientAdaptor : HttpClient, IHttpClient
        {
        }
    }
}