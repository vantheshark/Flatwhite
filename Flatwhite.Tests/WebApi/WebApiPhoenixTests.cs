using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiPhoenixTests
    {
        private readonly DummyController _controllerIntance = new DummyController();
        private readonly Flatwhite.WebApi.OutputCacheAttribute _cacheAttribute = new Flatwhite.WebApi.OutputCacheAttribute();
        private static readonly CacheInfo CacheInfo = new CacheInfo
        {
            CacheKey = "cacheKey",
            CacheStoreId = 0,
            CacheDuration = 100000,
            StaleWhileRevalidate = 100000
        };

        private static readonly Type controllerType = typeof (DummyController);

        [TestCase("HttpActionResult", 4)]
        [TestCase("HttpResponseMessageAsync", 4)]
        [TestCase("HttpResponseMessage", 4)]
        [TestCase("Object", 36)]
        [TestCase("String", 6)]
        [TestCase("StringAsync", 6)]
        [TestCase("Void", 6)]
        public void should_execute_the_controller_method_and_return_CacheItem(string actionMethodName, int contentLength)
        {
            // Arrange
            var currentCacheItem = new WebApiCacheItem();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[0]);
            invocation.Method.Returns(controllerType.GetMethod(actionMethodName, BindingFlags.Instance | BindingFlags.Public));

            var phoenix = new WebApiPhoenix(invocation, CacheInfo, currentCacheItem, new HttpRequestMessage(), new JsonMediaTypeFormatter());

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
