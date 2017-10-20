using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Flatwhite.WebApi.Owin.Controllers
{
    [Flatwhite.OutputCache(Duration = 2, StaleWhileRevalidate = 5)]
    public interface ICoffeeService
    {
        Task<string> OrderCoffeeAsync();

        string OrderCoffee();
    }

    public class FlatwhiteCoffeeService : ICoffeeService
    {
        private readonly List<string> _url = new List<string>
        {
            "https://en.wikipedia.org/wiki/Flat_white",
            "http://www.coffeehunter.org/flat-white-vs-latte/",
            "http://www.starbucks.com/menu/drinks/espresso/flat-white",
            "http://www.theguardian.com/lifeandstyle/2015/jan/05/what-is-a-flat-white-starbucks"
        };

        public string OrderCoffee()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            var index = rand.Next(_url.Count);
            var content = new WebClient().DownloadString(new Uri(_url[index]));
            return content;
        }
       
        public async Task<string> OrderCoffeeAsync()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            var index = rand.Next(_url.Count);
            var content = await new WebClient().DownloadStringTaskAsync(new Uri(_url[index]));
            return content;
        }
    }
}
