using System.Collections.Generic;
using Flatwhite.Strategy;
using Flatwhite.WebApi;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiCacheStrategyProviderTests
    {
        [Test]
        public void Should_return_strategy_for_webApi_by_context()
        {
            var context = new Dictionary<string, object>();
            var invocation = NSubstitute.Substitute.For<_IInvocation>();
            var provider = new WebApiCacheStrategyProvider();
            Assert.That(provider.GetStrategy(invocation, context) is DefaultCacheStrategy);

            context[WebApiExtensions.__webApi] = true;
            Assert.That(provider.GetStrategy(invocation, context) is WebApiCacheStrategy);
        }
    }
}
