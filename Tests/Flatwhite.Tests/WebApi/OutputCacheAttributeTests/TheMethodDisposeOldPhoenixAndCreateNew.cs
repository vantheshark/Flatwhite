using System;
using System.Net.Http;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodDisposeOldPhoenixAndCreateNew
    {
        private readonly _IInvocation _invocation = Substitute.For<_IInvocation>();
        private readonly HttpRequestMessage _request = UnitTestHelper.GetMessage();

        [SetUp]
        public void SetUp()
        {
            _invocation.Method.Returns(typeof(DummyController).GetMethod(nameof(DummyController.Object)));
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

            var existingPhoenix = Substitute.For<WebApiPhoenix>(_invocation, objCacheItem, _request);

            var att = new OutputCacheAttributeWithPublicMethods {MaxAge = 5, CacheStoreId = 1000, StaleWhileRevalidate = 5};

            Global.Cache.PhoenixFireCage[key] = existingPhoenix;

            // Action
            att.DisposeOldPhoenixAndCreateNew_Public(_invocation, objCacheItem, _request);

            // Assert
            Assert.That(Global.Cache.PhoenixFireCage[key] is WebApiPhoenix);
            existingPhoenix.Received(1).Dispose();
        }
    }
}
