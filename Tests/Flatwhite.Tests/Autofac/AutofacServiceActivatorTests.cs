using Autofac;
using Flatwhite.AutofacIntergration;
using NUnit.Framework;

namespace Flatwhite.Tests.Autofac
{
    [TestFixture]
    public class AutofacServiceActivatorTests
    {
        [Test]
        public void Should_try_to_create_instance_with_parameterless_constructor_if_type_is_not_registered()
        {
            var containerBuilder = new ContainerBuilder();
            var container = containerBuilder.Build();
            var activator = new AutofacServiceActivator(container);

            var logger = activator.CreateInstance(typeof (ConsoleLogger));

            Assert.IsNotNull(logger);
        }
    }
}
