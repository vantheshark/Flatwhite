using System.Collections.Generic;
using System.Net.Http;

namespace Flatwhite.WebApi.CacheControl
{
    /// <summary>
    /// A provider to get all registered <see cref="ICachControlHeaderHandler" /> from request
    /// </summary>
    public interface ICachControlHeaderHandlerProvider
    {
        /// <summary>
        /// Get all registered <see cref="ICachControlHeaderHandler" /> from request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IEnumerable<ICachControlHeaderHandler> Get(HttpRequestMessage request);

        /// <summary>
        /// Register a <see cref="ICachControlHeaderHandler" /> to current provider 
        /// </summary>
        /// <param name="handler"></param>
        void Register(ICachControlHeaderHandler handler);
    }
}
