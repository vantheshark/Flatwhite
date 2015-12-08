using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Flatwhite.Provider;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

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
            var scope = Substitute.For<IDependencyScope>();
            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods();

            // Action
            var startegy = att.GetCacheStrategyPublic(scope, invocation, invocationContext);

            // Assert
            Assert.That(startegy is WebApiCacheStrategy);
        }

        [Test]
        public void Should_try_to_resolve_by_type()
        {
            var scope = Substitute.For<IDependencyScope>();
            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = Substitute.For<ICacheStrategy>().GetType()
            };

            // Action
            var startegy = att.GetCacheStrategyPublic(scope, invocation, invocationContext);

            // Assert
            scope.Received(1).GetService(Arg.Is<Type>(t => t == att.CacheStrategyType));
            Assert.That(startegy is WebApiCacheStrategy);
        }

        [Test]
        public void Should_be_able_to_resolve_by_type_from_scope()
        {
            var scope = Substitute.For<IDependencyScope>();
            
            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = Substitute.For<ICacheStrategy>().GetType()
            };
            var expectedStrategy = Substitute.For<ICacheStrategy>();
            scope.GetService(Arg.Is<Type>(t => t == att.CacheStrategyType)).Returns(expectedStrategy);

            // Action
            var startegy = att.GetCacheStrategyPublic(scope, invocation, invocationContext);

            // Assert
            scope.Received(1).GetService(Arg.Is<Type>(t => t == att.CacheStrategyType));
            Assert.That(startegy == expectedStrategy);
        }

        [Test]
        public void Should_try_to_resolve_strategy_provider_from_scope_first()
        {
            var scope = Substitute.For<IDependencyScope>();

            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = Substitute.For<ICacheStrategy>().GetType()
            };

            var provider = Substitute.For<ICacheStrategyProvider>();
            var expectedStrategy = Substitute.For<ICacheStrategy>();
            provider.GetStrategy(Arg.Any<_IInvocation>(), Arg.Any<IDictionary<string, object>>()).Returns(expectedStrategy);
            scope.GetService(Arg.Is<Type>(t => t == typeof(ICacheStrategyProvider))).Returns(provider);

            // Action
            var startegy = att.GetCacheStrategyPublic(scope, invocation, invocationContext);

            // Assert
            scope.Received(1).GetService(Arg.Is<Type>(t => t == att.CacheStrategyType));
            Assert.That(startegy == expectedStrategy);
        }

        [Test]
        public void Should_throw_if_cannot_resolve_srategy()
        {
            var scope = Substitute.For<IDependencyScope>();

            var invocation = Substitute.For<_IInvocation>();
            var invocationContext = new Dictionary<string, object>
            {
                {WebApiExtensions.__webApi, true}
            };

            var att = new OutputCacheAttributeWithPublicMethods
            {
                CacheStrategyType = Substitute.For<ICacheStrategy>().GetType()
            };

            var provider = Substitute.For<ICacheStrategyProvider>();
            provider.GetStrategy(Arg.Any<_IInvocation>(), Arg.Any<IDictionary<string, object>>()).Returns((ICacheStrategy)null);
            scope.GetService(Arg.Is<Type>(t => t == typeof(ICacheStrategyProvider))).Returns(provider);

            // Action
            Assert.Throws<Exception>(() => { att.GetCacheStrategyPublic(scope, invocation, invocationContext); });
        }
    }
}
