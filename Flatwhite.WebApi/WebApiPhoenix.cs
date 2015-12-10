using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Flatwhite.Hot;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A web api phoenix to support auto refresh for webApi
    /// <para>
    /// It will create instance of WebApi controller and invoke the ActionMethod with cached arguments.
    /// It will not work if the controller required QueryString, Headers or anything outside these action method parameters
    /// But you can override the Phoenix MethodInfo, Arguments or method <see cref="Phoenix.GetTargetInstance" />, <see cref="Phoenix.InvokeAndGetBareResult" />, 
    /// or completely change the way the Phoenix reborn by overriding method , <see cref="Phoenix.Reborn" />,
    /// 
    /// Idealy, keep Controller thin and use proper Model binding instead of dodgy access the Request object.
    /// </para>
    /// </summary>
    public class WebApiPhoenix : Phoenix
    {
        private readonly WebApiCacheItem _cacheItem;
        private readonly MediaTypeFormatter _mediaTypeFormatter;
        private readonly HttpRequestMessage _clonedRequestMessage;
        private readonly HttpControllerDescriptor _controllerDescriptor;
        /// <summary>
        /// Initializes a WebApiPhoenix
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheItem">This should the the WebApiCacheItem instance</param>
        /// <param name="requestMessage"></param>
        /// <param name="mediaTypeFormatter">The formater used to create the HttpResponse if the return type of the action method is not a standard WebAPI action result</param>
        public WebApiPhoenix(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage requestMessage, MediaTypeFormatter mediaTypeFormatter = null) 
            : base(invocation, cacheItem)
        {
            _cacheItem = cacheItem;
            _mediaTypeFormatter = mediaTypeFormatter;
            _clonedRequestMessage = CloneRequest(requestMessage);
            _controllerDescriptor = requestMessage.GetActionDescriptor().ControllerDescriptor;
        }

        /// <summary>
        /// This Activator mainly used to resolve the WebApi controller instance
        /// </summary>
        protected override IServiceActivator Activator { get; } = WebApiExtensions._dependencyResolverActivator;

        /// <summary>
        /// Build the <see cref="WebApiCacheItem" /> from action result byte[] data
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override async Task<CacheItem> GetCacheItem(object response)
        {
            if (response == null)
            {
                return null;
            }
            HttpResponseMessage responseMsg = null;
            if (response is IHttpActionResult)
            {
                response = await ((IHttpActionResult)response).ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
            }
            else if (response is Task<HttpResponseMessage>)
            {
                responseMsg = await (Task<HttpResponseMessage>) response;
            }
            else
            {
                responseMsg = response as HttpResponseMessage;
            }

            if (responseMsg == null)
            {
                var httpContent = _mediaTypeFormatter != null
                    ? new ObjectContent(response.GetType(), response, _mediaTypeFormatter)
                    : (HttpContent) new StringContent((string)response);
                
                responseMsg = new HttpResponseMessage { Content = httpContent };
            }

            var content = await responseMsg.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            var newCacheItem = (WebApiCacheItem)_cacheItem.CloneWithoutData();
            newCacheItem.Content = content;
            newCacheItem.CreatedTime = DateTime.UtcNow;
            
            return newCacheItem;
        }

        /// <summary>
        /// Create the controller from _controllerDescriptor, it will by pass all ActionFilters
        /// </summary>
        /// <returns></returns>
        protected override object GetTargetInstance()
        {
            IHttpController controller = _controllerDescriptor.CreateController(_clonedRequestMessage);
            if (controller == null)
            {
                throw new Exception("Cannot recreate controller");
            }

            var apiController = controller as ApiController;
            if (apiController == null)
            {
                throw new NotSupportedException("controller must be ApiController");
            }

            apiController.ControllerContext = CreateControllerContext(_clonedRequestMessage, _controllerDescriptor, apiController);

            _clonedRequestMessage.Properties[HttpPropertyKeys.RequestContextKey] = apiController.RequestContext;
            _clonedRequestMessage.Properties[HttpPropertyKeys.SynchronizationContextKey] = SynchronizationContext.Current;
            apiController.Configuration = _controllerDescriptor.Configuration;
            apiController.Url = new UrlHelper(_clonedRequestMessage);
            apiController.Request = _clonedRequestMessage;

            return controller;
        }
     
        /// <summary>
        /// Clone a HttpRequestMessage but keys such as DependencyScope, SynchronizationContextKey
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual HttpRequestMessage CloneRequest(HttpRequestMessage original)
        {
            var clonedRequestMessage = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version,
                Headers = { },
                Properties = { {"__created_by", "Flatwhite.WebApi" } }
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

            //NOTE: HttpRequestContext seems to be a safe object to keep, well except IPrincipal. We don't suppose to cache data for Authorized request right?
            clonedRequestMessage.Properties.Remove(HttpPropertyKeys.DependencyScope);
            clonedRequestMessage.Properties.Remove(HttpPropertyKeys.DisposableRequestResourcesKey);
            clonedRequestMessage.Properties.Remove(HttpPropertyKeys.SynchronizationContextKey);

            return clonedRequestMessage;
        }

        /// <summary>
        /// Write cache updated for request
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected override void WriteCacheUpdatedLog()
        {
            Global.Logger.Info($"Updated key \"{_cacheItem.Key}\", store \"{_cacheItem.StoreId}\" for request {_clonedRequestMessage.RequestUri.PathAndQuery}");
        }

        /// <summary>
        /// Copied from System.Web.Http.Dispatcher.HttpControllerDispatcher
        /// </summary>
        /// <param name="request"></param>
        /// <param name="controllerDescriptor"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        private static HttpControllerContext CreateControllerContext(
            HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor,
            IHttpController controller)
        {
            HttpConfiguration controllerConfiguration = controllerDescriptor.Configuration;

            // Set the controller configuration on the request properties
            HttpConfiguration requestConfig = request.GetConfiguration();
            if (requestConfig == null)
            {
                request.SetConfiguration(controllerConfiguration);
            }
            else
            {
                if (requestConfig != controllerConfiguration)
                {
                    request.SetConfiguration(controllerConfiguration);
                }
            }

            HttpRequestContext requestContext = request.GetRequestContext();

            // if the host doesn't create the context we will fallback to creating it.

            if (requestContext == null)
            {
                //NOTE: Should not be null here
            }

            return new HttpControllerContext(requestContext, request, controllerDescriptor, controller);
        }
    }
}