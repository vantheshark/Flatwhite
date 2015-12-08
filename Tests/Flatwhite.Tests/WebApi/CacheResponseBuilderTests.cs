using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Flatwhite.Hot;
using Flatwhite.WebApi;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class CacheResponseBuilderTests
    {
        [Test]
        public void Should_return_GatewayTimeout_when_no_cache_and_request_sent_OnlyIfCached_control()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue {OnlyIfCached  = true};
            var svc = new CacheResponseBuilder();

            // Action
            var result = svc.GetResponse(cacheControl, null, new HttpRequestMessage());

            // Assert
            Assert.AreEqual(HttpStatusCode.GatewayTimeout, result.StatusCode);
            Assert.AreEqual("no cache available", result.Headers.GetValues("X-Flatwhite-Message").Single());
        }

        [Test]
        public void Should_return_null_when_no_cache_found()
        {
            // Arrange
            var svc = new CacheResponseBuilder();

            // Action
            var result = svc.GetResponse(new CacheControlHeaderValue { }, null, new HttpRequestMessage());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Should_return_null_if_stale_and_no_phoenix_created()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue
            {
                MaxStale = true,
                MaxStaleLimit = TimeSpan.FromSeconds(15)
            };

            var cacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow.AddSeconds(-10).AddMilliseconds(-1), // should stale just by 1 milisecond
                MaxAge = 10,
                StaleWhileRevalidate = 5,
                IgnoreRevalidationRequest = false,
                ResponseCharSet = "UTF8",
                ResponseMediaType = "text/json",
                Content = new byte[0],
                Key = "CacheKey" + Guid.NewGuid()
            };

            var request = new HttpRequestMessage();
            var svc = new CacheResponseBuilder { };

            // Action
            var response = svc.GetResponse(cacheControl, cacheItem, request);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void Should_return_null_if_stale()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue
            {
                MaxStale = true,
                MaxStaleLimit = TimeSpan.FromSeconds(15)
            };

            var cacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow.AddSeconds(-10).AddMilliseconds(-1), // should stale just by 1 milisecond
                MaxAge = 10,
                StaleWhileRevalidate = 5,
                IgnoreRevalidationRequest = false,
                ResponseCharSet = "UTF8",
                ResponseMediaType = "text/json",
                Content = new byte[0],
                Key = "CacheKey" + Guid.NewGuid()
            };

            Global.Cache.PhoenixFireCage[cacheItem.Key] = new Phoenix(NSubstitute.Substitute.For<_IInvocation>(), new CacheInfo());

            var request = new HttpRequestMessage();
            var svc = new CacheResponseBuilder { };

            // Action
            var response = svc.GetResponse(cacheControl, cacheItem, request);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void Should_return_null_if_cache_not_mature_as_min_fresh_request()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue
            {
                MinFresh = TimeSpan.FromSeconds(100)
            };

            var cacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow.AddSeconds(-20),
                MaxAge = 1000,
                StaleWhileRevalidate = 5,
                IgnoreRevalidationRequest = false
            };

            var request = new HttpRequestMessage();
            var svc = new CacheResponseBuilder { };

            // Action
            var response = svc.GetResponse(cacheControl, cacheItem, request);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void Should_return_Stale_header_if_stale_by_max_age()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue
            {
                MaxStale = true,
                MaxStaleLimit = TimeSpan.FromSeconds(15),
                MinFresh = TimeSpan.FromSeconds(20)
            };

            var cacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow.AddSeconds(-11),
                MaxAge = 10,
                StaleWhileRevalidate = 5,
                IgnoreRevalidationRequest = true,
                ResponseCharSet = "UTF8",
                ResponseMediaType = "text/json",
                Content = new byte[0],
                Key = "CacheKey" + Guid.NewGuid()
            };

            Global.Cache.PhoenixFireCage[cacheItem.Key] = new Phoenix(NSubstitute.Substitute.For<_IInvocation>(), new CacheInfo());

            var request = new HttpRequestMessage();
            var svc = new CacheResponseBuilder { };

            // Action
            var response = svc.GetResponse(cacheControl, cacheItem, request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Cache freshness lifetime not qualified",   response.Headers.GetValues("X-Flatwhite-Warning").First());
            Assert.AreEqual("Response is Stale",                        response.Headers.GetValues("X-Flatwhite-Warning").Last());
            Assert.AreEqual($"110 - \"Response is Stale\"", response.Headers.GetValues("Warning").Last());
            Assert.AreEqual("stale-while-revalidate", response.Headers.CacheControl.Extensions.ToList().First().Name);
            Assert.AreEqual(cacheItem.ResponseMediaType, response.Content.Headers.ContentType.MediaType);
            Assert.AreEqual(cacheItem.ResponseCharSet, response.Content.Headers.ContentType.CharSet);
        }

        [Test]
        public void Should_return_304_if_etag_matched()
        {
            // Arrange
            var cacheControl = new CacheControlHeaderValue
            {
                MaxStale = true,
                MaxStaleLimit = TimeSpan.FromSeconds(15),
                MinFresh = TimeSpan.FromSeconds(20)
            };

            var cacheItem = new WebApiCacheItem
            {
                CreatedTime = DateTime.UtcNow.AddSeconds(-11),
                MaxAge = 10,
                StaleWhileRevalidate = 5,
                IgnoreRevalidationRequest = true,
                ResponseCharSet = "UTF8",
                ResponseMediaType = "text/json",
                Content = new byte[0],
                Key = "CacheKey" + Guid.NewGuid()
            };

            Global.Cache.PhoenixFireCage[cacheItem.Key] = new Phoenix(NSubstitute.Substitute.For<_IInvocation>(), new CacheInfo());

            var request = new HttpRequestMessage();
            request.Properties[WebApiExtensions.__webApi_etag_matched] = true;
            var svc = new CacheResponseBuilder { };

            // Action
            var response = svc.GetResponse(cacheControl, cacheItem, request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotModified, response.StatusCode);
            Assert.AreEqual("Cache freshness lifetime not qualified", response.Headers.GetValues("X-Flatwhite-Warning").First());
            Assert.AreEqual("Response is Stale", response.Headers.GetValues("X-Flatwhite-Warning").Last());
            Assert.AreEqual($"110 - \"Response is Stale\"", response.Headers.GetValues("Warning").Last());
        }
    }
}
