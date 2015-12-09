using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiPhoenixTests
    {
        private readonly DummyController _controllerIntance = new DummyController();
        private static readonly Type controllerType = typeof (DummyController);

        [SetUp]
        public void SetUp()
        {
            Global.Init();
            WebApiExtensions._dependencyResolverActivator = new ServiceActivator();
        }

        [TestCase(nameof(DummyController.HttpActionResult), 4)]
        [TestCase(nameof(DummyController.HttpResponseMessageAsync), 4)]
        [TestCase(nameof(DummyController.HttpResponseMessage), 4)]
        [TestCase(nameof(DummyController.Object), 36)]
        [TestCase(nameof(DummyController.String), 6)]
        [TestCase(nameof(DummyController.StringAsync), 6)]
        [TestCase(nameof(DummyController.Void), 6)]
        public void should_execute_the_controller_method_and_return_CacheItem(string actionMethodName, int contentLength)
        {
            // Arrange
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(controllerType.GetMethod(actionMethodName, BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenix(invocation, currentCacheItem, new HttpRequestMessage(), new JsonMediaTypeFormatter());

            // Action
            MethodInfo dynMethod = typeof(WebApiPhoenix).GetMethod("InvokeAndGetBareResult", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = dynMethod.Invoke(phoenix, new object[] { _controllerIntance });

            dynMethod = typeof(WebApiPhoenix).GetMethod("GetCacheItem", BindingFlags.NonPublic | BindingFlags.Instance);
            var cacheItem = (WebApiCacheItem)dynMethod.Invoke(phoenix, new[] { result });


            // Assert
            if (result == null)
            {
                Assert.IsTrue(actionMethodName == "Void" || actionMethodName == "VoidAsync");
            }
            else
            {
                Assert.AreEqual(contentLength, cacheItem.Content.Length);
            }
        }

        [Test]
        public void GetTargetInstance_should_set_the_request()
        {
            // Arrange
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(controllerType.GetMethod(nameof(DummyController.Object), BindingFlags.Instance | BindingFlags.Public));
            var requestMsg = new HttpRequestMessage
            {
                Headers = { {"key", "Value"} },
                Method = HttpMethod.Get,
                Properties =
                {
                    {"p1", "v1"},
                    { HttpPropertyKeys.DependencyScope, Substitute.For<IDependencyScope>()},
                    { HttpPropertyKeys.DisposableRequestResourcesKey, new List<IDisposable>() },
                    { HttpPropertyKeys.SynchronizationContextKey, SynchronizationContext.Current }
                },
                RequestUri = new Uri("http://localhost/api")
            };
            var phoenix = new WebApiPhoenix(invocation, currentCacheItem, requestMsg, new JsonMediaTypeFormatter());

            // Action
            MethodInfo dynMethod = typeof(WebApiPhoenix).GetMethod("GetTargetInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            var controller = dynMethod.Invoke(phoenix, new object[] { }) as ApiController;

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.Request);
            Assert.AreEqual(1, controller.Request.Headers.Count());
            Assert.AreEqual(3, controller.Request.Properties.Count()); // 1 created by the Phoenix, 1 is the 
        }
    }

    public class DummyController : ApiController
    {
        public void Void() { }

        public Task VoidAsync() { return Task.Delay(0); }

        public string String()
        {
            return "data";
        }

        public object Object()
        {
            return new
            {
                Name = "Van",
                Project = "Flatwhite"
            };
        }

        public Task<string> StringAsync()
        {
            return Task.FromResult("data");
        }

        public HttpResponseMessage HttpResponseMessage()
        {
            return new HttpResponseMessage
            {
                Content = new StringContent("data")
            };
        }

        public Task<HttpResponseMessage> HttpResponseMessageAsync()
        {
            return Task.FromResult(new HttpResponseMessage { Content = new StringContent("data") });
        }

        public IHttpActionResult HttpActionResult()
        {
            return new CustomHttpActionResult();
        }

        private class CustomHttpActionResult : IHttpActionResult
        {
            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage { Content = new StringContent("data") });
            }
        }
    }
}
