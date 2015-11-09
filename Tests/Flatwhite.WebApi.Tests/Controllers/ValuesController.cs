using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Flatwhite.WebApi.Tests.Controllers
{
    public class ValuesController : ApiController
    {
        [OutputCache(Duration = 2000)]
        public virtual async Task<IEnumerable<string>> Get()
        {
            await Task.Delay(2000);
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        [OutputCache(Duration = 2000)]
        public virtual async Task<HttpResponseMessage> Get(string id)
        {
            var content = await new WebClient().DownloadStringTaskAsync(new Uri($"https://www.nuget.org/packages?q={id}"));
            return new HttpResponseMessage()
            {
                Content = new StringContent(content,Encoding.UTF8,"text/html")
            };
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
