using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Flatwhite.WebApi.Owin.Controllers
{
    public class TestRevalidateController : ApiController
    {
        /// <summary>
        /// Cache data for this method is created seperately
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getfavourites")]
        [WebApi.OutputCache(MaxAge = 2, StaleWhileRevalidate = 10, RevalidateKeyFormat = "GetFavourites", IgnoreRevalidationRequest = true)]
        public IHttpActionResult GetFavourites()
        {
            return Ok(DateTime.Now.ToLongTimeString());
        }

        /// <summary>
        /// Cache data for this method is created seperately
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/getall")]
        [WebApi.OutputCache(MaxAge = 2, StaleWhileRevalidate = 10, RevalidateKeyFormat = "GetAll", IgnoreRevalidationRequest = true)]
        public IHttpActionResult GetAll()
        {
            return Ok(DateTime.Now.ToLongTimeString());
        }

        /// <summary>
        /// This is the POST method to clear caches from those 2 endpoints
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/setfavourites")]
        [WebApi.Revalidate("GetFavourites", "GetAll")]
        public HttpResponseMessage SetFavourites()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// This is the GET method to clear caches from those 2 endpoints
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/reset-cache")]
        [WebApi.Revalidate("GetFavourites", "GetAll")]
        public virtual HttpResponseMessage ResetCache()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
