using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flatwhite.WebApi.CacheControl;
using Microsoft.Owin.Logging;

namespace Flatwhite.WebApi
{
    /// http://www.asp.net/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api
    /// <summary>
    /// This is a custom WebApi message handler which try to build the response if cache data is available
    /// This should create a response asap if there is the cache without waiting for the <see cref="OutputCacheAttribute" /> to do that which is quite late
    /// and quite heavy "if your controller has too many dependencies". People should try to not having heavy Controller anyway. 
    /// </summary>
    public class CacheMessageHandler : DelegatingHandler
    {
        private readonly ICachControlHeaderHandlerProvider _handlerProvider;

        /// <summary>
        /// Initializes an instance of <see cref="CacheMessageHandler" /> from a provided <see cref="ICachControlHeaderHandlerProvider" />
        /// </summary>
        /// <param name="handlerProvider"></param>
        public CacheMessageHandler(ICachControlHeaderHandlerProvider handlerProvider)
        {
            if (handlerProvider == null)
            {
                throw new ArgumentNullException(nameof(handlerProvider));
            }
            _handlerProvider = handlerProvider;
        }

        /// <summary>
        /// Process the request and attempt to build the response from cache
        /// If it failed, it simply logs the error and continue the pipeline
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // TODO: Ignore static files if WebAPI hosted with MVC web app. Probably keep it like this for now as everything will be changed once vNext release

            var handlers = _handlerProvider.Get(request);
            foreach (var handler in handlers)
            {
                try
                {
                    var result = await handler.HandleAsync(request.Headers.CacheControl, request, cancellationToken);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Global.Logger.Error("Error during handling the request", ex);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
