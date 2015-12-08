using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    public class TheMethodOnActionExecutingAsync
    {
        private readonly _IInvocation _invocation = Substitute.For<_IInvocation>();
        private HttpRequestMessage _request;
        private HttpActionContext _actionContext;

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
            var discriptor = Substitute.For<ReflectedHttpActionDescriptor>();
            discriptor.MethodInfo = _invocation.Method;
            discriptor.ReturnType.Returns(typeof(object));



            _actionContext = new HttpActionContext(
                new HttpControllerContext(
                    new HttpConfiguration(),
                    Substitute.For<IHttpRouteData>(), _request), discriptor);

            Global.Init();
        }

        [Test]
        public async Task Should_call_reborn_on_existing_phoenix_and_try_refresh_cache_when_cache_item_is_stale()
        {
            // Arrange
            var store = Substitute.For<IAsyncCacheStore>();
            store.StoreId.Returns(1000);
            var objCacheItem = new WebApiCacheItem
            {
                MaxAge = 5,
                StaleWhileRevalidate = 5,
                StoreId = 1000,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
            };

            var existingPhoenix = Substitute.For<WebApiPhoenix>(_invocation, 
                new CacheInfo
                {
                    CacheStoreId = 1000,
                    CacheKey = objCacheItem.Key,
                    CacheDuration = 5,
                    StaleWhileRevalidate = 5
                }, 
                objCacheItem, _request, null);
            existingPhoenix.When(x => x.Reborn()).Do(c =>
            {
                Global.Cache.PhoenixFireCage.Remove(objCacheItem.Key);
            });

            store.GetAsync(Arg.Any<string>()).Returns(c =>
            {
                objCacheItem.Key = c.Arg<string>();
                Global.Cache.PhoenixFireCage[objCacheItem.Key] = existingPhoenix;
                return Task.FromResult((object)objCacheItem);
            });

            Global.CacheStoreProvider.RegisterAsyncStore(store);
            var att = new Flatwhite.WebApi.OutputCacheAttribute { MaxAge = 5, CacheStoreId = 1000, StaleWhileRevalidate = 5 };

            // Action
            await att.OnActionExecutingAsync(_actionContext, CancellationToken.None);

            // Assert
            existingPhoenix.Received(1).Reborn();
        }

        [Test]
        public async Task Should_create_phoenix_and_try_refresh_cache_when_cache_item_is_stale()
        {
            // Arrange
            var store = Substitute.For<IAsyncCacheStore>();
            store.StoreId.Returns(1000);
            var objCacheItem = new WebApiCacheItem
            {
                MaxAge = 5,
                StaleWhileRevalidate = 5,
                StoreId = 1000,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
            };

            store.GetAsync(Arg.Any<string>()).Returns(c =>
            {
                objCacheItem.Key = c.Arg<string>();
                return Task.FromResult((object)objCacheItem);
            });

            Global.CacheStoreProvider.RegisterAsyncStore(store);
            var att = new Flatwhite.WebApi.OutputCacheAttribute { MaxAge = 5, CacheStoreId = 1000, StaleWhileRevalidate = 5 };

            // Action
            await att.OnActionExecutingAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.IsTrue(Global.Cache.PhoenixFireCage.ContainsKey(objCacheItem.Key));
        }
    }
}
