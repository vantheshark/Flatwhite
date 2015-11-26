using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Flatwhite.Hot;

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
            _cacheKey = info.CacheKey;
            _mediaTypeFormatter = mediaTypeFormatter;
            _clonedRequestMessage = CloneRequest(requestMessage);
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

        /// <summary>
        /// Create the controller and set Request instance
        /// </summary>
        /// <returns></returns>
        protected override object GetTargetInstance()
        {
            var controller = base.GetTargetInstance() as ApiController;
            if (controller != null)
            {
                TODO: It does not work here
                controller.Request = _clonedRequestMessage;
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
    }
}