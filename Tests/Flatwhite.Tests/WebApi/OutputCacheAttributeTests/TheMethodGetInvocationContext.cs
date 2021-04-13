using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Routing;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodGetInvocationContext
    {
        [Test]
        public void Should_return_context_with_webApi_related_keys()
        {
            // Arrange
            var att = new OutputCacheAttributeWithPublicMethods();
            var dependencyScope = Substitute.For<IDependencyScope>();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost/api"),
                Properties = { { "MS_DependencyScope", dependencyScope } }, //https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Web.Http/Hosting/HttpPropertyKeys.cs
                Headers = { },
                Content = new StringContent("Content...")
            };

            var actionContext = new HttpActionContext(
                new HttpControllerContext(
                    new HttpConfiguration(),
                    Substitute.For<IHttpRouteData>(), request),
                Substitute.For<HttpActionDescriptor>());

            // Action
            var context = att.GetInvocationContextPublic(actionContext);

            // Assert
            Assert.AreSame(att, context[Global.__flatwhite_outputcache_attribute]);
            Assert.IsTrue((bool)context[WebApiExtensions.__webApi]);
            Assert.IsNotNull(context["headers"]);
            Assert.IsNotNull(context["method"]);
            Assert.IsNotNull(context["requestUri"]);
            Assert.IsNotNull(context["query"]);
        }
    }
}
