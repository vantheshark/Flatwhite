using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Dependencies;
using Flatwhite.Provider;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [TestFixture]
    public class TheMethodGetCacheStrategy
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStrategyProvider = new WebApiCacheStrategyProvider();
        }

        [Test]
        public void Should_return_WebApiCacheStrategy_by_default()
        {
            // Arrange
            var request = CreateHttpRequestMessage();
            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods();

            // Action
            var startegy = att.GetCacheStrategyPublic(request, invocation, invocationContext);

            // Assert
            Assert.That(startegy is WebApiCacheStrategy);
        }

        [Test]
        public void Should_be_able_to_resolve_by_type_from_scope()
        {
            var scop = Substitute.For<IDependencyScope>();
            var request = CreateHttpRequestMessage(scop);

            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };
            var cacheStrategy = Substitute.For<ICacheStrategy>();
            scop.GetService(cacheStrategy.GetType()).Returns(cacheStrategy);
            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = cacheStrategy.GetType()
            };

            // Action
            var startegy = att.GetCacheStrategyPublic(request, invocation, invocationContext);

            // Assert
            Assert.AreSame(cacheStrategy, startegy);
            scop.Received(1).GetService(Arg.Is<Type>(t => t == att.CacheStrategyType));
        }


        [Test]
        public void Should_throw_if_cannot_resolve_srategy_from_scope()
        {
            var request = CreateHttpRequestMessage();
            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };
            var cacheStrategy = Substitute.For<ICacheStrategy>();

            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = cacheStrategy.GetType()
            };

            // Action
            Assert.Throws<Exception>(() => { att.GetCacheStrategyPublic(request, invocation, invocationContext); });
        }

        [Test]
        public void Should_be_able_to_resolve_from_provider_when_CacheStrategyType_not_available()
        {
            var request = CreateHttpRequestMessage();

            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };
            
            var att = new OutputCacheAttributeWithPublicMethods();

            // Action
            var startegy = att.GetCacheStrategyPublic(request, invocation, invocationContext);

            // Assert
            Assert.IsInstanceOf<WebApiCacheStrategy>(startegy);
        }

        [Test]
        public void Should_throw_exception_if_cannot_find_cache_strategy()
        {
            var request = CreateHttpRequestMessage();

            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods();
            Global.CacheStrategyProvider = Substitute.For<ICacheStrategyProvider>();
            Global.CacheStrategyProvider.GetStrategy(Arg.Any<_IInvocation>(), Arg.Any<IDictionary<string, object>>()).Returns(default(ICacheStrategy));

            // Action
            Assert.Throws<Exception>(() => { att.GetCacheStrategyPublic(request, invocation, invocationContext); });
        }

        private static HttpRequestMessage CreateHttpRequestMessage(IDependencyScope dependencyScope = null)
        {
            var request = Substitute.For<HttpRequestMessage>();
            request.Properties["MS_DependencyScope"] = dependencyScope ?? Substitute.For<IDependencyScope>();
            request.RequestUri = new Uri("http://server.com/api");
            return request;
        }
    }
}
