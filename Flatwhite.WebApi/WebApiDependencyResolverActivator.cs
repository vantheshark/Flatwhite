using System;
using System.Web.Http.Dependencies;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// This class will use WebApi DependencyResolver to resolve the controllers
    /// </summary>
    internal class WebApiDependencyResolverActivator : IServiceActivator
    {
        private readonly Func<IDependencyScope> _dependencyResolver;

        public WebApiDependencyResolverActivator(Func<IDependencyScope> dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public object CreateInstance(Type serviceType)
        {
            return _dependencyResolver().GetService(serviceType);
        }
    }
}
