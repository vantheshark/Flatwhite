using System;
using System.Web.Http.Dependencies;
using Flatwhite.WebApi;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.WebApi
{
    [TestFixture]
    public class WebApiDependencyResolverActivatorTests
    {
        [Test]
        public void Should_delegate_service_resolve_call_to_IDependencyScope()
        {
            var dependencyScope = Substitute.For<IDependencyScope>();
            Func<IDependencyScope> func = () => dependencyScope;
            var activator = new WebApiDependencyResolverActivator(func);

            activator.CreateInstance(typeof (IUserService));

            dependencyScope.Received(1).GetService(typeof (IUserService));
        }
    }
}
