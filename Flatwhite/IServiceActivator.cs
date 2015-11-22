using System;

namespace Flatwhite
{
    /// <summary>
    /// Provide method to create instance of service to refresh the stale cache
    /// </summary>
    public interface IServiceActivator
    {
        /// <summary>
        /// Create an instance of a service by type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object CreateInstance(Type serviceType);
    }

    internal class ServiceActivator : IServiceActivator
    {
        public object CreateInstance(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }
    }
}
