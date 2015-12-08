using System;
using System.Collections.Generic;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiCacheStrategyTests
    {
        public int MethodWithReturnType()
        {
            return 0;
        }

        [Test]
        public void Should_throw_when_attempt_to_get_sync_cache_store()
        {
            var strategy = new WebApiCacheStrategy();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(WebApiCacheStrategyTests).GetMethod(nameof(MethodWithReturnType)));

            var context = new Dictionary<string, object> {};

            Assert.IsTrue(strategy.CanCache(invocation, context));
            Assert.Throws<NotSupportedException>(() => { strategy.GetCacheStore(invocation, context); });
        }
    }
}
