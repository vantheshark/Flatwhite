using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Flatwhite.WebApi.Tests.Controllers
{
    public class ValuesController : ApiController
    {
        [OutputCache(
            MaxAge = 10,
            StaleWhileRevalidate = 5,
            VaryByParam = "packageId",
            RevalidationKey = "VaryByParamMethod",
            IgnoreRevalidationRequest = true,
            AutoRefresh = true)]
        public virtual async Task<IEnumerable<string>> Get()
        {
            await Task.Delay(2000);
            return new[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/vary-by-param-async/{packageId}")]
        [OutputCache(
            MaxAge = 10,
            StaleWhileRevalidate = 5,
            VaryByParam = "packageId",
            RevalidationKey = "VaryByParamMethod",
            IgnoreRevalidationRequest = true)]
        public async Task<HttpResponseMessage> VaryByParamAsync(string packageId)
        {
            var sw = Stopwatch.StartNew();
            var content = await new WebClient().DownloadStringTaskAsync(new Uri($"https://www.nuget.org/packages/" + packageId));
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} Milliseconds", Encoding.UTF8, "text/html")
            };
        }

        [HttpGet]
        [Route("api/vary-by-param/{packageId}")]
        [OutputCache(
            MaxAge = 10, 
            StaleWhileRevalidate = 5, 
            VaryByParam = "packageId", 
            RevalidationKey = "VaryByParamMethod",
            IgnoreRevalidationRequest = true)]
        public HttpResponseMessage VaryByParam(string packageId)
        {
            var sw = Stopwatch.StartNew();
            var content = new WebClient().DownloadString(new Uri($"https://www.nuget.org/packages/" + packageId));
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} Milliseconds", Encoding.UTF8,"text/html")
            };
        }

        [HttpGet]
        [Route("api/string/{packageId}")]
        [OutputCache(
            MaxAge = 5,
            StaleWhileRevalidate = 5,
            VaryByParam = "packageId",
            IgnoreRevalidationRequest = true)]
        public string String(string packageId)
        {
            return packageId;
        }

        [HttpGet]
        [Route("api/vary-by-header")]
        [OutputCache(MaxAge = 10, StaleWhileRevalidate = 5, VaryByHeader = "UserAgent")]
        public virtual async Task<HttpResponseMessage> VaryByCustom()
        {
            var sw = Stopwatch.StartNew();
            var content = await new WebClient().DownloadStringTaskAsync(new Uri($"https://www.nuget.org/packages/Flatwhite"));
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} Milliseconds", Encoding.UTF8, "text/html")
            };
        }


        [HttpGet]
        [Route("api/reset")]
        [Revalidate("VaryByParamMethod")]
        public virtual HttpResponseMessage ResetCache()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
