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
    public class TheMethodGetCacheResponseBuilder
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStrategyProvider = new WebApiCacheStrategyProvider();
        }

        [Test]
        public void Should_return_CacheResponseBuilder_by_default()
        {
            // Arrange
            var scope = Substitute.For<IDependencyScope>();
            var att = new OutputCacheAttributeWithPublicMethods();

            // Action
            var builder = att.GetCacheResponseBuilderPublic(scope);

            // Assert
            Assert.That(builder is CacheResponseBuilder);
        }

        [Test]
        public void Should_be_able_to_resolve_from_scope()
        {
            // Arrange
            var scope = Substitute.For<IDependencyScope>();
            var expectedBuilder = Substitute.For<ICacheResponseBuilder>();
            scope.GetService(Arg.Is<Type>(t => t == typeof (ICacheResponseBuilder))).Returns(expectedBuilder);
            var att = new OutputCacheAttributeWithPublicMethods();

            // Action
            var builder = att.GetCacheResponseBuilderPublic(scope);

            // Assert
            Assert.That(builder == expectedBuilder);
        }
    }
}
