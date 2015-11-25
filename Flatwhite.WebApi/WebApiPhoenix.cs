using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A web api phoenix to support auto refresh for webApi
    /// <para>
    /// It will create instance of WebApi controller and invoke the ActionMethod with cached arguments.
    /// It will not work if the controller required QueryString, Headers or anything outside these action method parameters
    /// You can override the Phoenix MethodInfo, Arguments or method <see cref="Phoenix.GetTargetInstance" />, <see cref="Phoenix.GetMethodResult" />, 
    /// or completly change the way the Phoenix reborn by overriding method , <see cref="Phoenix.Reborn(object)" />,
    /// 
    /// Idealy, keep Controller thin and use proper Model binding instead of dodgyly access the Request object.
    /// </para>
    /// </summary>
    public class WebApiPhoenix : Phoenix
    {
        private readonly string _cacheKey;
        private readonly MediaTypeFormatter _mediaTypeFormatter;

        /// <summary>
        /// Initializes a WebApiPhoenix
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheStoreId"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="staleWhileRevalidate"></param>
        /// <param name="outputCache">This should the the OutputCacheAttribute isntance</param>
        /// <param name="mediaTypeFormatter">The formater used to create the HttpResponse if the return type of the action method is not a standard WebAPI action result</param>
        public WebApiPhoenix(_IInvocation invocation, int cacheStoreId, string cacheKey, int cacheDuration, int staleWhileRevalidate, OutputCacheAttribute outputCache, MediaTypeFormatter mediaTypeFormatter = null) 
            : base(invocation, cacheStoreId, cacheKey, cacheDuration, staleWhileRevalidate, outputCache)
        {
            _cacheKey = cacheKey;
            _mediaTypeFormatter = mediaTypeFormatter;
        }

        /// <summary>
        /// This Activator mainly used to resolve the WebApi controller instance
        /// </summary>
        protected override IServiceActivator Activator { get; } = WebApiExtensions._dependencyResolverActivator;

        /// <summary>
        /// Invoke the MethodInfo against the Controller and try to get the action method result
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="outputCache">The OutputCache itself</param>
        /// <returns></returns>
        protected override object GetMethodResult(object serviceInstance, object outputCache)
        {
            var response = base.GetMethodResult(serviceInstance, outputCache);

            if (response == null)
            {
                return null;
            }

            if (response is IHttpActionResult)
            {
                Task.Run(async () =>
                {
                    response = await ((IHttpActionResult) response).ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
                }).GetAwaiter().GetResult();
            }

            var responseMsg = response as HttpResponseMessage;

            if (responseMsg == null)
            {
                responseMsg = new HttpResponseMessage
                {
                    Content = new ObjectContent(response.GetType(), response, _mediaTypeFormatter)
                };
            }
            
            var responseContent = responseMsg.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return new CacheItem((OutputCacheAttribute)outputCache)
            {
                Key = _cacheKey,
                Content = responseContent,
                ResponseMediaType = responseMsg.Content.Headers.ContentType.MediaType,
                ResponseCharSet = responseMsg.Content.Headers.ContentType.CharSet
            };
        }
    }
}