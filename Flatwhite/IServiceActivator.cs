using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Flatwhite
{
    /// <summary>
    /// Provide method to create instance of service to refresh the stale cache
    /// </summary>
    public interface ICacheDependencyScope : IDisposable
    {
        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object CreateInstance(Type serviceType);
    }

    /// <summary>
    /// Provide method to start a dependency scope
    /// </summary>
    public interface IServiceActivator : ICacheDependencyScope
    {
        /// <summary>
        /// Create a dependency scope
        /// </summary>
        /// <returns></returns>
        ICacheDependencyScope BeginScope();
    }

    [ExcludeFromCodeCoverage]
    internal class DefaultServiceActivator : IServiceActivator
    {
        public object CreateInstance(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public ICacheDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
