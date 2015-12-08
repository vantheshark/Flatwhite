using System;
using System.Linq;
using System.Net.Http;
using Flatwhite.WebApi.CacheControl;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi.CacheControl
{
    [TestFixture]
    public class CachControlHeaderHandlerProviderTests
    {
        [Test]
        public void Can_register_and_get_ICachControlHeaderHandler()
        {
            // Arrange
            var handler = Substitute.For<ICachControlHeaderHandler>();
            var provider = new CachControlHeaderHandlerProvider();
            provider.Register(handler);


            // Action
            var handlers = provider.Get(new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/api")));

            // Assert
            Assert.Throws<ArgumentNullException>(() => provider.Register(null));
            Assert.That(handlers.ToList().Contains(handler));
        }
    }
}
