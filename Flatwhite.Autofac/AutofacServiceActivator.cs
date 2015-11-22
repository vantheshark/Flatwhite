using System;
using Autofac;

namespace Flatwhite.AutofacIntergration
{
    internal class AutofacServiceActivator : IServiceActivator
    {
        private readonly IComponentContext _container;

        public AutofacServiceActivator(IComponentContext container)
        {
            _container = container;
        }

        public object CreateInstance(Type serviceType)
        {
            if (_container.IsRegistered(serviceType))
            {
                return _container.Resolve(serviceType);
            }
            return Activator.CreateInstance(serviceType);
        }
    }
}
