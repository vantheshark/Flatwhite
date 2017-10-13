using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodOnActionExecutedAsync
    {
        private readonly _IInvocation _invocation = Substitute.For<_IInvocation>();
        private HttpRequestMessage _request;
        private HttpActionContext _actionContext;
        private HttpActionExecutedContext _actionExecutedContext;
        private IAsyncCacheStore _store;
        private static string CacheKey = "the_cache_key";

        [SetUp]
        public void SetUp()
        {
            _invocation.Method.Returns(typeof(DummyController).GetMethod(nameof(DummyController.Object)));
            _request = UnitTestHelper.GetMessage();
            var discriptor = Substitute.For<ReflectedHttpActionDescriptor>();
            discriptor.MethodInfo = _invocation.Method;
            discriptor.ReturnType.Returns(typeof(object));


            _actionContext = new HttpActionContext(
                new HttpControllerContext(
                    new HttpConfiguration(),
                    Substitute.For<IHttpRouteData>(), _request), discriptor);

            _store = Substitute.For<IAsyncCacheStore>();
            _store.StoreId.Returns(1000);

            _actionExecutedContext = new HttpActionExecutedContext(_actionContext, null);
            _actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_store] = _store;
            _actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_key] = CacheKey;

            Global.Init();
            Global.CacheStoreProvider.RegisterAsyncStore(_store);
        }

        [TestCase(0, "", true, 5, "{}", HttpStatusCode.OK)]
        [TestCase(60, "no-cache", false, 5, "{}", HttpStatusCode.OK)]
        [TestCase(60, "no-store", false, 5, "{}", HttpStatusCode.OK)]
        [TestCase(60, "max-age=0", false, 5, "{}", HttpStatusCode.OK)]
        [TestCase(60, "", false, 5, null, HttpStatusCode.OK)]
        [TestCase(60, "", false, 0, "{}", HttpStatusCode.InternalServerError)]
        public async Task Test_none_cachable_cases(int attMaxAge, string requestCacheHeader, bool attIgnoreRevalidationRequest, int attStaleIfError, string respone, HttpStatusCode responseCode)
        {
            // Arrange
            _request.Headers.CacheControl =
                new CacheControlHeaderValue
                {
                    NoCache = requestCacheHeader.Contains("no-cache"),
                    NoStore = requestCacheHeader.Contains("no-store"),
                    MaxAge = requestCacheHeader.Contains("max-age=0") ?  TimeSpan.Zero : TimeSpan.FromSeconds(10)
                };

            var att = new Flatwhite.WebApi.OutputCacheAttribute {
                MaxAge = (uint)attMaxAge,
                IgnoreRevalidationRequest = attIgnoreRevalidationRequest,
                StaleIfError = (uint)attStaleIfError
            };

            _actionExecutedContext.Response = !string.IsNullOrEmpty(respone) ? new HttpResponseMessage(responseCode)
            {
                Content = new StringContent(respone)
            } : null;

            // Action
            await att.OnActionExecutedAsync(_actionExecutedContext, CancellationToken.None);

            // Assert
            await _store.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DateTimeOffset>());
        }

        [Test]
        public async Task Should_remove_self_refresh_header()
        {
            // Arrange
            _request.Headers.CacheControl =
                new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromSeconds(10),
                    Extensions = { new NameValueHeaderValue(WebApiExtensions.__cacheControl_flatwhite_force_refresh, "1")}
                };

            var att = new Flatwhite.WebApi.OutputCacheAttribute
            {
                MaxAge = 10,
                IgnoreRevalidationRequest = true,
                StaleIfError = 2,
                CacheStoreId = _store.StoreId
            };

            _actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

            // Action
            await att.OnActionExecutedAsync(_actionExecutedContext, CancellationToken.None);

            // Assert
            //await _store.Received(2).SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DateTimeOffset>());
            Assert.IsFalse(_request.Headers.CacheControl.Extensions.Any(x => x.Name == WebApiExtensions.__cacheControl_flatwhite_force_refresh));
        }

        [Test]
        public async Task Should_response_current_cache_if_there_is_error_but_StaleIfError_is_not_zero()
        {
            // Arrange
            _request.Headers.CacheControl =
                new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromSeconds(10),
                    Extensions = { new NameValueHeaderValue(WebApiExtensions.__cacheControl_flatwhite_force_refresh, "1") }
                };

            var att = new Flatwhite.WebApi.OutputCacheAttribute
            {
                MaxAge = 10,
                IgnoreRevalidationRequest = true,
                StaleIfError = 2
            };

            _actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var objCacheItem = new WebApiCacheItem
            {
                MaxAge = 10,
                Key = CacheKey,
                StaleWhileRevalidate = 5,
                StoreId = _store.StoreId,
                StaleIfError = 2,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
                Content = new byte[] {0, 1, 2},
                ResponseMediaType = "application/json",
                ResponseCharSet = "utf-8",
                IgnoreRevalidationRequest = true
            };
            _store.GetAsync(CacheKey).Returns(objCacheItem);

            // Action
            await att.OnActionExecutedAsync(_actionExecutedContext, CancellationToken.None);

            // Assert
            Assert.IsTrue(_actionExecutedContext.Response.IsSuccessStatusCode);
            await _store.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DateTimeOffset>());
        }

        [Test]
        public async Task Should_save_result_to_cache_and_create_new_phoenix()
        {
            // Arrange
            _request.Headers.CacheControl =
                new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromSeconds(10),
                    Extensions = { new NameValueHeaderValue(WebApiExtensions.__cacheControl_flatwhite_force_refresh, "1") }
                };

            var att = new Flatwhite.WebApi.OutputCacheAttribute
            {
                MaxAge = 10,
                IgnoreRevalidationRequest = true,
                StaleIfError = 2,
                AutoRefresh = true
            };

            _actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

            var objCacheItem = new WebApiCacheItem
            {
                Key = CacheKey,
                MaxAge = 5,
                StoreId = 1000,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
            };
            var existingPhoenix = Substitute.For<WebApiPhoenix>(_invocation, objCacheItem, _request);
            Global.Cache.PhoenixFireCage[CacheKey] = existingPhoenix;

            // Action
            await att.OnActionExecutedAsync(_actionExecutedContext, CancellationToken.None);

            // Assert
            await _store.Received(2).SetAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DateTimeOffset>());
            Assert.AreNotSame(existingPhoenix, Global.Cache.PhoenixFireCage[CacheKey]);
        }


        [Test]
        public async Task Should_hook_up_change_monitors_if_StaleWhileRevalidate_greater_than_0()
        {
            // Arrange
            var wait = new AutoResetEvent(false);
            _request.Headers.CacheControl =
                new CacheControlHeaderValue
                {
                    MaxAge = TimeSpan.FromSeconds(10),
                    Extensions = { new NameValueHeaderValue(WebApiExtensions.__cacheControl_flatwhite_force_refresh, "1") }
                };

            var att = new Flatwhite.WebApi.OutputCacheAttribute
            {
                MaxAge = 10,
                IgnoreRevalidationRequest = true,
                StaleIfError = 2,
                StaleWhileRevalidate = 5
            };

            _actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

            var objCacheItem = new WebApiCacheItem
            {
                Key = CacheKey,
                MaxAge = 5,
                StaleWhileRevalidate = 5,
                StoreId = 1000,
                CreatedTime = DateTime.UtcNow.AddSeconds(-5).AddMilliseconds(-1),
            };
            var existingPhoenix = Substitute.For<WebApiPhoenix>(_invocation, objCacheItem, _request);
            Global.Cache.PhoenixFireCage[CacheKey] = existingPhoenix;
            existingPhoenix.When(x => x.Reborn()).Do(c =>
            {
                wait.Set();
            });

            var strategy = Substitute.For<ICacheStrategy>();
            var changeMonitor = Substitute.For<IChangeMonitor>();
            strategy.GetChangeMonitors(Arg.Any<_IInvocation>(), Arg.Any<IDictionary<string, object>>()).Returns(new[] { changeMonitor });
            _actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_strategy] = strategy;
            

            // Action
            await att.OnActionExecutedAsync(_actionExecutedContext, CancellationToken.None);
            changeMonitor.CacheMonitorChanged += Raise.Event<CacheMonitorChangeEvent>(new object());

            // Assert
            await Task.Delay(1000);
            Assert.IsTrue(wait.WaitOne(TimeSpan.FromSeconds(2)));
        }
    }
}
