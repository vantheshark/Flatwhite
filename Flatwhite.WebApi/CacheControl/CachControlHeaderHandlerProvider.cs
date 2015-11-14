using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Flatwhite.WebApi.CacheControl
{
    /// <summary>
    /// Default implementation of <see cref="ICachControlHeaderHandlerProvider" /> using a static <see cref="List{ICachControlHeaderHandlerProvider}" />
    /// </summary>
    public class CachControlHeaderHandlerProvider : ICachControlHeaderHandlerProvider
    {
        private static readonly List<ICachControlHeaderHandler> _registeredHandlers = new List<ICachControlHeaderHandler>();

        /// <summary>
        /// Get all registered <see cref="ICachControlHeaderHandlerProvider" /> from this provider
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IEnumerable<ICachControlHeaderHandler> Get(HttpRequestMessage request)
        {
            return _registeredHandlers;
        }

        /// <summary>
        /// Register a <see cref="ICachControlHeaderHandlerProvider" /> to this provider
        /// </summary>
        /// <param name="handler"></param>
        public void Register(ICachControlHeaderHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            _registeredHandlers.Add(handler);
        }
    }
}