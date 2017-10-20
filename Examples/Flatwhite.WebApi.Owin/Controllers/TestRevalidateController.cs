using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Flatwhite.WebApi.Owin.Controllers
{
    public class TestRevalidateController : ApiController
    {
        [HttpGet]
        [Route("api/getfavourites")]
        [WebApi.OutputCache(MaxAge = 2, StaleWhileRevalidate = 10, RevalidateKeyFormat = "GetFavourites", IgnoreRevalidationRequest = true)]
        public IHttpActionResult GetFavourites()
        {
            return Ok(DateTime.Now.ToLongTimeString());
        }

        [HttpGet]
        [Route("api/getall")]
        [WebApi.OutputCache(MaxAge = 2, StaleWhileRevalidate = 10, RevalidateKeyFormat = "GetAll", IgnoreRevalidationRequest = true)]
        public IHttpActionResult GetAll()
        {
            return Ok(DateTime.Now.ToLongTimeString());
        }


        [HttpPost]
        [Route("api/setfavourites")]
        [WebApi.Revalidate("GetFavourites", "GetAll")]
        public HttpResponseMessage SetFavourites()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        [HttpGet]
        [Route("api/reset-cache")]
        [WebApi.Revalidate("GetFavourites", "GetAll")]
        public virtual HttpResponseMessage ResetCache()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
