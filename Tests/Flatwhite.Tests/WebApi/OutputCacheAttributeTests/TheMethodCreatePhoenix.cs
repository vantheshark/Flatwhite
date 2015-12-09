using System;
using System.Net.Http;
using System.Web.Http.Dependencies;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodCreatePhoenix
    {
        private readonly _IInvocation _invocation = Substitute.For<_IInvocation>();
        private HttpRequestMessage _request;

        [SetUp]
        public void SetUp()
        {
            _invocation.Method.Returns(typeof(DummyController).GetMethod(nameof(DummyController.Object)));
            var dependencyScope = Substitute.For<IDependencyScope>();
            _request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost/api"),
                Properties = { { "MS_DependencyScope", dependencyScope } }, //https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Web.Http/Hosting/HttpPropertyKeys.cs
                Headers = { },
                Content = new StringContent("Content...")
            };
            Global.Init();
        }

        [Test]
        public void Should_dispose_existing_phoenix()
        {
            var key = "theCacheKey" + Guid.NewGuid();
            // Arrange
            var objCacheItem = new WebApiCacheItem
            {
                MaxAge = 5,
                StaleWhileRevalidate = 5,
                StoreId = 1000,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
                Key = key
            };

            var existingPhoenix = Substitute.For<WebApiPhoenix>(_invocation, objCacheItem, _request, null);

            var att = new OutputCacheAttributeWithPublicMethods {MaxAge = 5, CacheStoreId = 1000, StaleWhileRevalidate = 5};

            Global.Cache.PhoenixFireCage[key] = existingPhoenix;

            // Action
            att.CreatePhoenixPublic(_invocation, objCacheItem, _request, null);

            // Assert
            Assert.That(Global.Cache.PhoenixFireCage[key] is WebApiPhoenix);
            existingPhoenix.Received(1).Dispose();
        }
    }
}
