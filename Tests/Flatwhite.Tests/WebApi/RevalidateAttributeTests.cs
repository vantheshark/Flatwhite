using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class RevalidateAttributeTests
    {
        [Test]
        public void Test_OnActionExecuted()
        {
            var att = new Flatwhite.WebApi.RevalidateAttribute("User");
            var httpActionExecutedContext = GetHttpActionExecutedContext();

            var str = "";
            Global.RevalidateEvent += s => { str = s; };
            att.OnActionExecuted(httpActionExecutedContext);
            Assert.AreEqual("User", str);
        }

        [Test]
        public void Test_OnActionExecuted_should_do_nothing_if_Response_is_failed()
        {
            var att = new Flatwhite.WebApi.RevalidateAttribute("User");
            var httpActionExecutedContext = GetHttpActionExecutedContext();
            httpActionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            var str = "";
            Global.RevalidateEvent += s => { str = s; };
            att.OnActionExecuted(httpActionExecutedContext);
            Assert.AreEqual("", str);
        }

        [Test]
        public async Task Test_OnActionExecutedAsync()
        {
            var att = new Flatwhite.WebApi.RevalidateAttribute("User");
            var httpActionExecutedContext = GetHttpActionExecutedContext();

            var str = "";
            Global.RevalidateEvent += s => { str = s; };
            await att.OnActionExecutedAsync(httpActionExecutedContext, CancellationToken.None);
            Assert.AreEqual("User", str);
        }

        [Test]
        public async Task Test_OnActionExecutedAsync_should_do_nothing_if_Response_is_failed()
        {
            var att = new Flatwhite.WebApi.RevalidateAttribute("User");
            var httpActionExecutedContext = GetHttpActionExecutedContext();
            httpActionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);

            var str = "";
            Global.RevalidateEvent += s => { str = s; };
            await att.OnActionExecutedAsync(httpActionExecutedContext, CancellationToken.None);
            Assert.AreEqual("", str);
        }


        private static HttpActionExecutedContext GetHttpActionExecutedContext(MethodInfo info = null)
        {
            var actionDescriptor = Substitute.For<ReflectedHttpActionDescriptor>();
            
            actionDescriptor.MethodInfo = info ?? Substitute.For<MethodInfo>();

            var httpActionExecutedContext = new HttpActionExecutedContext(
                new HttpActionContext(
                    new HttpControllerContext(
                        new HttpConfiguration(),
                        Substitute.For<IHttpRouteData>(),
                        new HttpRequestMessage()),
                    actionDescriptor),
                null);
            httpActionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.OK);
            return httpActionExecutedContext;
        }
    }
}
