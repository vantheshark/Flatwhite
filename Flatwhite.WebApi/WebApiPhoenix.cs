using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Flatwhite.Hot;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A web api phoenix to support auto refresh for webApi
    /// <para>
    /// It will create instance of WebApi controller and invoke the ActionMethod with cached arguments.
    /// It will not work if the controller required QueryString, Headers or anything outside these action method parameters
    /// But you can override the Phoenix MethodInfo, Arguments or method <see cref="Phoenix.GetTargetInstance" />, <see cref="Phoenix.InvokeAndGetBareResult" />, 
    /// or completely change the way the Phoenix reborn by overriding method , <see cref="Phoenix.Reborn(object)" />,
    /// 
    /// Idealy, keep Controller thin and use proper Model binding instead of dodgy access the Request object.
    /// </para>
    /// </summary>
    public class WebApiPhoenix : Phoenix
    {
        private readonly CacheInfo _info;
        private readonly MediaTypeFormatter _mediaTypeFormatter;
        private readonly HttpRequestMessage _clonedRequestMessage;

        /// <summary>
        /// Initializes a WebApiPhoenix
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="info"></param>
        /// <param name="outputCache">This should the the OutputCacheAttribute isntance</param>
        /// <param name="requestMessage"></param>
        /// <param name="mediaTypeFormatter">The formater used to create the HttpResponse if the return type of the action method is not a standard WebAPI action result</param>
        public WebApiPhoenix(_IInvocation invocation, CacheInfo info , OutputCacheAttribute outputCache, HttpRequestMessage requestMessage, MediaTypeFormatter mediaTypeFormatter = null) 
            : base(invocation, info, outputCache)
        {
            _info = info;
            _mediaTypeFormatter = mediaTypeFormatter;
            _clonedRequestMessage = CloneRequest(requestMessage);
        }

        /// <summary>
        /// This Activator mainly used to resolve the WebApi controller instance
        /// </summary>
        protected override IServiceActivator Activator { get; } = WebApiExtensions._dependencyResolverActivator;

        /// <summary>
        /// Build the <see cref="WebApiCacheItem" /> from action result byte[] data
        /// </summary>
        /// <param name="response"></param>
        /// <param name="outputCache"></param>
        /// <returns></returns>
        protected override CacheItem GetCacheItem(object response, object outputCache)
        {
            if (response == null)
            {
                return null;
            }

            if (response is IHttpActionResult)
            {
                Task.Run(async () =>
                {
                    response = await ((IHttpActionResult)response).ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
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

            var content = responseMsg.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            
            return new WebApiCacheItem((OutputCacheAttribute)outputCache)
            {
                Key = _info.CacheKey,
                StoreId = _info.CacheStoreId,
                Content = content,
                ResponseMediaType = responseMsg.Content.Headers.ContentType.MediaType,
                ResponseCharSet = responseMsg.Content.Headers.ContentType.CharSet
            };
        }

        /// <summary>
        /// Create the controller and set Request instance
        /// </summary>
        /// <returns></returns>
        protected override object GetTargetInstance()
        {
            var controller = base.GetTargetInstance() as ApiController;
            if (controller != null)
            {
                controller.ControllerContext.Request = _clonedRequestMessage;
                // Not support other stuff for now
            }
            return controller;
        }

        /// <summary>
        /// Clone a HttpRequestMessage
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual HttpRequestMessage CloneRequest(HttpRequestMessage original)
        {
            var clonedRequestMessage = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version,
                Headers = { },
                Properties = { {"__created_by", "Flatwhite.Api"} }
            };
            foreach (var k in original.Headers)
            {
                clonedRequestMessage.Headers.Add(k.Key, k.Value);
            }
            foreach (var k in original.Properties)
            {
                if (!k.Key.StartsWith("__flatwhite"))
                {
                    clonedRequestMessage.Properties.Add(k.Key, k.Value);
                }
            }
            return clonedRequestMessage;
        }

        /// <summary>
        /// Write cache updated for request
        /// </summary>
        protected override void WriteCacheUpdatedLog()
        {
            Global.Logger.Info($"Updated key \"{_info.CacheKey}\", store \"{_info.CacheStoreId}\" for request {_clonedRequestMessage.RequestUri.PathAndQuery}");
        }
    }
}