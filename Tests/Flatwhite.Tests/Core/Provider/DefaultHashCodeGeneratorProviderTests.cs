using System;
using Flatwhite.Provider;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Provider
{
    [TestFixture]
    public class DefaultHashCodeGeneratorProviderTests
    {
        [Test]
        public void Should_return_DefaultHashCodeGenerator_if_type_is_unregistered()
        {
            var provider = new DefaultHashCodeGeneratorProvider();

            Assert.IsTrue(provider.GetForType(typeof(DefaultHashCodeGeneratorProviderTests)) is DefaultHashCodeGenerator);
        }

        [Test]
        public void Should_throw_exception_if_register_a_null_hashCodeGenerator()
        {
            var provider = new DefaultHashCodeGeneratorProvider();

            Assert.Throws<ArgumentNullException>(() => provider.Register<IHashCodeGenerator>(null));
        }
    }
}
