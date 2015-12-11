using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Provides a base class for sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
    /// </summary>
    public interface IHttpClient : IDisposable
    {
        /// <summary>
        /// Gets or sets the timespan to wait before the request times out.
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="completionOption">When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
        /// <returns>Returns System.Threading.Tasks.Task`1.The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);
    }
}