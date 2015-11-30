using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiContextProviderTests
    {
        [Test]
        public void The_method_get_context_should_return_context_with_headers_methods_and_uri()
        {
            // Arrange
            var controllerContext = new HttpControllerContext
            {
                Request = new System.Net.Http.HttpRequestMessage
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri("http://localhost")
                }
            };
            var httpActionContext = new HttpActionContext(controllerContext, Substitute.For<HttpActionDescriptor>());
            
            var provider = new WebApiContextProvider(httpActionContext);
            
            // Action
            var result = provider.GetContext();
            Assert.IsNotNull(result.TryGetByKey<HttpRequestHeaders>("headers"));
            Assert.IsNotNull(result.TryGetByKey<HttpMethod>("method"));
            Assert.IsNotNull(result.TryGetByKey<Uri>("requestUri"));
            Assert.IsTrue(result.TryGetByKey<bool>(WebApiExtensions.__webApi));
        }
    }
}
