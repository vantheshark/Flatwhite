using System.Net;
using System.Net.Http;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodApplyCacheHeaders
    {
        [Test]
        public void Should_return_WebApiCacheStrategy_by_default()
        {
            // Arrange
            var att = new OutputCacheAttributeWithPublicMethods
            {
                MaxAge = 1,
                SMaxAge = 2,
                MustRevalidate = true,
                ProxyRevalidate = true,
                Private = true,
                Public = true,
                NoStore = true,
                NoCache = true,
                NoTransform = true
            };

            var request = new HttpRequestMessage();
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Action
            att.ApplyCacheHeadersPublic(response, request);
            var control = response.Headers.CacheControl;
            
            // Assert
            Assert.AreEqual(1, control.MaxAge.Value.Seconds);
            Assert.AreEqual(2, control.SharedMaxAge.Value.Seconds);
            Assert.IsTrue(control.MustRevalidate);
            Assert.IsTrue(control.ProxyRevalidate);
            Assert.IsTrue(control.Private);
            Assert.IsTrue(control.Public);
            Assert.IsTrue(control.NoCache);
            Assert.IsTrue(control.NoStore);
            Assert.IsTrue(control.NoTransform);

            Assert.AreEqual("no-cache", response.Headers.Pragma.ToString());
        }
    }
}
