using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using NSubstitute;

namespace Flatwhite.Tests.WebApi
{
    internal static class UnitTestHelper
    {
        public static HttpRequestMessage GetMessage()
        {
            var httpConfiguration = Substitute.For<HttpConfiguration>();
            var mockControlerDescriptor = Substitute.For<HttpControllerDescriptor>();
            mockControlerDescriptor.CreateController(Arg.Any<HttpRequestMessage>()).Returns(new DummyController());
            mockControlerDescriptor.Configuration = httpConfiguration;

            var requestContext = Substitute.For<HttpRequestContext>();
            requestContext.Configuration.Returns(httpConfiguration);

            return new HttpRequestMessage
            {
                Headers = {{"key", "Value"}},
                Method = HttpMethod.Get,
                Properties =
                {
                    {"p1", "v1"},
                    {HttpPropertyKeys.RequestContextKey, requestContext},
                    {HttpPropertyKeys.DependencyScope, Substitute.For<IDependencyScope>()}, //https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Web.Http/Hosting/HttpPropertyKeys.cs
                    {HttpPropertyKeys.DisposableRequestResourcesKey, new List<IDisposable>()},
                    {HttpPropertyKeys.SynchronizationContextKey, SynchronizationContext.Current},
                    {HttpPropertyKeys.HttpActionDescriptorKey, new ReflectedHttpActionDescriptor { ControllerDescriptor = mockControlerDescriptor}}
                },
                RequestUri = new Uri("http://localhost/api/method/id")
            };
        }
    }
}
