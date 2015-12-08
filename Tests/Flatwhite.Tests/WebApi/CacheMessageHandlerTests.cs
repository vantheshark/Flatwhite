using System;
using Flatwhite.WebApi;
using Flatwhite.WebApi.CacheControl;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    public class CacheMessageHandlerTests
    {
        [Test]
        public void Should_throw_if_ICachControlHeaderHandlerProvider_is_null()
        {
            // Arrange
            Assert.Throws<ArgumentNullException>(() => new CacheMessageHandler(null));

            // NOTE: Should be fine
            new CacheMessageHandler(Substitute.For<ICachControlHeaderHandlerProvider>());
        }
    }
}
