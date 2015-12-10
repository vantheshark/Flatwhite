using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Flatwhite.Tests.WebApi
{
    public class DummyController : ApiController
    {
        public void Void() { }

        public Task VoidAsync() { return Task.Delay(0); }

        public string String()
        {
            return "data";
        }

        public object Object()
        {
            return new
            {
                Name = "Van",
                Project = "Flatwhite"
            };
        }

        public async Task<string> StringAsync()
        {
            using (var httpClient = new WebClient())
            {
                var content = await httpClient.DownloadStringTaskAsync("https://www.nuget.org/");
                return content;
            }
        }

        public HttpResponseMessage HttpResponseMessage()
        {
            return new HttpResponseMessage
            {
                Content = new StringContent("data")
            };
        }

        public async Task<HttpResponseMessage> HttpResponseMessageAsync()
        {
            using (var httpClient = new WebClient())
            {
                var content = await httpClient.DownloadStringTaskAsync("https://www.nuget.org/");
                return new HttpResponseMessage {Content = new StringContent(content)};
            }
        }

        public IHttpActionResult HttpActionResult()
        {
            return new CustomHttpActionResult();
        }

        private class CustomHttpActionResult : IHttpActionResult
        {
            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                using (var httpClient = new WebClient())
                {
                    var content = await httpClient.DownloadStringTaskAsync("https://www.nuget.org/");
                    return new HttpResponseMessage {Content = new StringContent(content) };
                }
            }
        }
    }
}