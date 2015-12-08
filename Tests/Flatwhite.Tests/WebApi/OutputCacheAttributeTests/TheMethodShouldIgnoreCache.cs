using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Flatwhite.WebApi;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodShouldIgnoreCache
    {
        [TestCase(0, null, null, null, true, ExpectedResult = true, Description = "Ignore cache because maxAge = 0")]
        [TestCase(1, null, null, null, true, ExpectedResult = false, Description = "maxAge > 0")]
        [TestCase(1, true, null, null, false, ExpectedResult = true, Description = "cacheControl.noCache is true")]
        [TestCase(1, null, true, null, false, ExpectedResult = true, Description = "cacheControl.noStore is true")]
        [TestCase(1, null, null, 0, false, ExpectedResult = true, Description = "cacheControl.maxAge == 0")]

        [TestCase(1, true, null, null, true, ExpectedResult = false, Description = "cacheControl.noCache is true but IgnoreRevalidationRequest is true")]
        [TestCase(1, null, true, null, true, ExpectedResult = false, Description = "cacheControl.noStore is true but IgnoreRevalidationRequest is true")]
        [TestCase(1, null, null, 0, true, ExpectedResult = false, Description = "cacheControl.maxAge == 0 but IgnoreRevalidationRequest is true")]

        public bool Should_be_determined_from_MaxAge_and_cacheControl(int maxAge, bool? noCache, bool? noStore, int? cacheControlMaxAge, bool ignoreRevalidation)
        {
            CacheControlHeaderValue cacheControl = null;
            if (noCache.HasValue || noStore.HasValue || cacheControlMaxAge.HasValue)
            {
                cacheControl = new CacheControlHeaderValue
                {
                    NoCache = noCache ?? false,
                    NoStore = noStore ?? false,
                };

                if (cacheControlMaxAge.HasValue)
                {
                    cacheControl.MaxAge = TimeSpan.FromSeconds(cacheControlMaxAge.Value);
                }
            }
            var att = new OutputCacheAttributeWithPublicMethods
            {
                MaxAge = (uint)maxAge,
                IgnoreRevalidationRequest = ignoreRevalidation
            };

            // Action
            return att.ShouldIgnoreCachePublic(cacheControl, new HttpRequestMessage());
        }

        [Test]
        public void Should_return_true_if_request_property_has_key_telling_no_cache()
        {
            var att = new OutputCacheAttributeWithPublicMethods
            {
                MaxAge = 10
            };
            var request = new HttpRequestMessage
            {
                Properties = {{$"{WebApiExtensions.__flatwhite_dont_cache}_for_test", true}}
            };

            // Action
            Assert.IsTrue(att.ShouldIgnoreCachePublic(new CacheControlHeaderValue(), request));
        }

    }
}
