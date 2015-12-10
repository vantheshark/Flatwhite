using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;

namespace Flatwhite.WebApi2.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly ILog _logger;
        private static string _hint = "<br /><br /> <strong>Go to <a href='/_flatwhite/store/0' target='_blank'>/_flatwhite/store/{storeId}</a> to see cache statuses</strong>" +
                                      "<br /><br /> <strong>Go to <a href='/_flatwhite/phoenix' target='_blank'>/_flatwhite/phoenix</a> to see phoenix statuses</strong>";
        public ValuesController(ILog logger)
        {
            _logger = logger;
        }

        public ValuesController() : this (LogManager.GetLogger(typeof(ValuesController)))
        {
        }

        [WebApi.OutputCache(
            MaxAge = 3,
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
        [WebApi.OutputCache(
            MaxAge = 3,
            StaleWhileRevalidate = 5,
            VaryByParam = "packageId",
            RevalidationKey = "VaryByParamMethod",
            IgnoreRevalidationRequest = true,
            AutoRefresh = true)]
        public async Task<HttpResponseMessage> VaryByParamAsync(string packageId)
        {
            var sw = Stopwatch.StartNew();
            var content = await new WebClient().DownloadStringTaskAsync(new Uri($"https://www.nuget.org/packages/" + packageId));
            sw.Stop();
            _logger.Info($"{nameof(VaryByParam)} Elapsed {sw.ElapsedMilliseconds} milliseconds");
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} milliseconds.{_hint}", Encoding.UTF8, "text/html")
            };
        }



        [HttpGet]
        [Route("api/vary-by-param/{packageId}")]
        [WebApi.OutputCache(
            MaxAge = 3, 
            StaleWhileRevalidate = 5, 
            VaryByParam = "packageId", 
            RevalidationKey = "VaryByParamMethod",
            IgnoreRevalidationRequest = true,
            AutoRefresh = true)]
        public HttpResponseMessage VaryByParam(string packageId)
        {
            var sw = Stopwatch.StartNew();
            var content = new WebClient().DownloadString(new Uri($"https://www.nuget.org/packages/" + packageId));
            sw.Stop();
            _logger.Info($"{nameof(VaryByParam)} Elapsed {sw.ElapsedMilliseconds} milliseconds");
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} milliseconds.{_hint}", Encoding.UTF8,"text/html")
            };
        }


        [HttpGet]
        [Route("api/string/{packageId}")]
        [WebApi.OutputCache(
            MaxAge = 3,
            StaleWhileRevalidate = 5,
            VaryByParam = "packageId",
            IgnoreRevalidationRequest = true)]
        public string String(string packageId)
        {
            return packageId;
        }



        [HttpGet]
        [Route("api/vary-by-header")]
        [WebApi.OutputCache(MaxAge = 3, StaleWhileRevalidate = 5, VaryByHeader = "UserAgent, CacheControl.Public", VaryByCustom = "query.src")]
        public virtual async Task<HttpResponseMessage> VaryByCustom()
        {
            var sw = Stopwatch.StartNew();
            var content = await new WebClient().DownloadStringTaskAsync(new Uri($"https://www.nuget.org/packages/Flatwhite"));
            sw.Stop();
            _logger.Info($"{nameof(VaryByParam)} Elapsed {sw.ElapsedMilliseconds} milliseconds.{_hint}");
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Elapsed {sw.ElapsedMilliseconds} milliseconds", Encoding.UTF8, "text/html")
            };
        }

        [HttpGet]
        [Route("api/reset")]
        [WebApi.Revalidate("VaryByParamMethod")]
        public virtual HttpResponseMessage ResetCache()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
