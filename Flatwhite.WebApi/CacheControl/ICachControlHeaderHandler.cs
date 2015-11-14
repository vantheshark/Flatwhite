using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Flatwhite.WebApi.CacheControl
{
    /// <summary>
    /// Provide a single method to try to build a <see cref="HttpResponseHeaders" /> from <see cref="CacheControlHeaderValue" /> and <see cref="HttpRequestMessage" />
    /// </summary>
    public interface ICachControlHeaderHandler
    {
        /// <summary>
        /// This method will try to build a <see cref="HttpResponseHeaders" /> from <see cref="CacheControlHeaderValue" /> and <see cref="HttpRequestMessage" />
        /// </summary>
        /// <param name="cacheControl"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> HandleAsync(CacheControlHeaderValue cacheControl, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}