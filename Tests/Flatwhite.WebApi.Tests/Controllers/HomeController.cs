using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flatwhite.WebApi.Tests.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 2000)]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return new ContentResult();
        }
    }
}
