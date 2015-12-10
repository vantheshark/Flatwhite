using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

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

        public Task<string> StringAsync()
        {
            return Task.FromResult("data");
        }

        public HttpResponseMessage HttpResponseMessage()
        {
            return new HttpResponseMessage
            {
                Content = new StringContent("data")
            };
        }

        public Task<HttpResponseMessage> HttpResponseMessageAsync()
        {
            return Task.FromResult(new HttpResponseMessage { Content = new StringContent("data") });
        }

        public IHttpActionResult HttpActionResult()
        {
            return new CustomHttpActionResult();
        }

        private class CustomHttpActionResult : IHttpActionResult
        {
            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage { Content = new StringContent("data") });
            }
        }
    }
}