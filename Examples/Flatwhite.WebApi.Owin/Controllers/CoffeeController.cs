using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Flatwhite.WebApi2.Controllers;
using log4net;

namespace Flatwhite.WebApi.Owin.Controllers
{
    /// <summary>
    /// This controller call service cached by Flatwhite
    /// </summary>
    public class CoffeeController : ApiController
    {
        private readonly ICoffeeService _coffeeService;
        private readonly ILog _logger;
        private static string _hint = "<br /><br /> <strong>Go to <a href='/_flatwhite/store/0' target='_blank'>/_flatwhite/store/{storeId}</a> to see cache statuses</strong>" +
                                      "<br /><br /> <strong>Go to <a href='/_flatwhite/phoenix' target='_blank'>/_flatwhite/phoenix</a> to see phoenix statuses</strong>";
        public CoffeeController(ICoffeeService coffeeService, ILog logger)
        {
            _coffeeService = coffeeService;
            _logger = logger;
        }

        public CoffeeController() : this (new FlatwhiteCoffeeService(), LogManager.GetLogger(typeof(ValuesController)))
        {
        }


        [HttpGet]
        [Route("api/cache-on-interface-async")]
        public async Task<HttpResponseMessage> CacheOnInterfaceAsync()
        {
            var sw = Stopwatch.StartNew();
            await _coffeeService.OrderCoffeeAsync();
            sw.Stop();
            _logger.Info($"ActionMethod {nameof(CacheOnInterfaceAsync)} elapsed {sw.ElapsedMilliseconds} ms");
            return new HttpResponseMessage()
            {
                Content = new StringContent($"Download elapsed {sw.ElapsedMilliseconds} milliseconds.{_hint}", Encoding.UTF8, "text/html")
            };
        }
        

        [HttpGet]
        [Route("api/cache-on-interface")]
        public HttpResponseMessage CacheOnInterface()
        {
            var sw = Stopwatch.StartNew();
            _coffeeService.OrderCoffee();
            sw.Stop();
            _logger.Info($"ActionMethod {nameof(CacheOnInterface)} elapsed {sw.ElapsedMilliseconds} ms");
            return new HttpResponseMessage()
            {
                Content = new StringContent($"elapsed {sw.ElapsedMilliseconds} milliseconds.{_hint}", Encoding.UTF8,"text/html")
            };
        }
    }
}
