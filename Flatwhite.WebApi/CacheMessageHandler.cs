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
    /// </summary>
    public class CacheMessageHandler : DelegatingHandler
    {
        private readonly ICachControlHeaderHandlerProvider _handlerProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes an instance of <see cref="CacheMessageHandler" /> from a provided <see cref="ICachControlHeaderHandlerProvider" />
        /// </summary>
        /// <param name="handlerProvider"></param>
        /// <param name="logger"></param>
        public CacheMessageHandler(ICachControlHeaderHandlerProvider handlerProvider, ILogger logger)
        {
            if (handlerProvider == null)
            {
                throw new ArgumentNullException(nameof(handlerProvider));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _handlerProvider = handlerProvider;
            _logger = logger;
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
            // TODO: Ignore static files

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
                    _logger.WriteError("Error during handling the request", ex);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
