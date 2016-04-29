using System;
using Autofac;
using Autofac.Core;

namespace Flatwhite.AutofacIntergration
{
    internal class AutofacServiceActivator : IServiceActivator
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacServiceActivator(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public object CreateInstance(Type serviceType)
        {
            if (_lifetimeScope.IsRegistered(serviceType))
            {
                return _lifetimeScope.Resolve(serviceType);
            }
            return Activator.CreateInstance(serviceType);
        }

        public ICacheDependencyScope BeginScope()
        {
            return new LifeTimeScopeAdaptor(_lifetimeScope.BeginLifetimeScope());
        }

        public void Dispose()
        {
        }
    }

    internal class LifeTimeScopeAdaptor : ICacheDependencyScope
    {
        private readonly ILifetimeScope _autoLifetimeScope;

        public LifeTimeScopeAdaptor(ILifetimeScope autoLifetimeScope)
        {
            _autoLifetimeScope = autoLifetimeScope;
        }

        public void Dispose()
        {
            _autoLifetimeScope.Dispose();
        }

        public object CreateInstance(Type serviceType)
        {
            return _autoLifetimeScope.ResolveService(new TypedService(serviceType));
        }
    }
}
