using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Flatwhite.Provider;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Provider
{
    [TestFixture]
    public class DefaultCacheKeyProviderTests
    {
        [Test]
        public void Should_build_key_from_methodInfo_and_context()
        {
            // Arrange
            var invocation = Substitute.For<_IInvocation>();
            invocation.Arguments.Returns(new object[] {new Guid ("5e31d440-c9f8-4110-b94d-fdf8affdc675") });
            invocation.GetArgumentValue(Arg.Any<int>()).Returns(ct => invocation.Arguments[ct.Arg<int>()]);
            invocation.Method.Returns(typeof(IUserService).GetMethod(nameof(IUserService.GetById), BindingFlags.Instance | BindingFlags.Public));

            var provider = new DefaultCacheKeyProvider();
            var invocationContext = new Dictionary<string, object>();
            invocationContext[Global.__flatwhite_outputcache_attribute] = new OutputCacheAttribute
            {
                VaryByCustom = "query.source, headers.UserAgent, headers.CacheControl.Public, headers.CacheControl.NonExistProperty",
                VaryByParam = "userId"
            };
            invocationContext["query"] = new Dictionary<string, object>
            {
                { "source","a"},
                { "source2","b"},
            };
            var r = new HttpRequestMessage();
            r.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true
            };
            r.Headers.Add("User-Agent", "Flatwhite.UnitTest");
            invocationContext["headers"] = r.Headers;

            // Action
            var key = provider.GetCacheKey(invocation, invocationContext);

            // Assert
            Assert.AreEqual("Flatwhite::Flatwhite.Tests.IUserService.GetById(Guid:5e31d440-c9f8-4110-b94d-fdf8affdc675) ::  query.source:a,  headers.UserAgent:Flatwhite.UnitTest,  headers.CacheControl.Public:True, , ", key);
        }

        [Test]
        public void Should_throw_exception_if_CacheSettings_not_found_in_context()
        {
            var invocation = Substitute.For<_IInvocation>();
            var provider = new DefaultCacheKeyProvider();
            var invocationContext = new Dictionary<string, object>();

            Assert.Throws<InvalidOperationException>(() => provider.GetCacheKey(invocation, invocationContext));
        }
    }
}
