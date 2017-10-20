using System;
using System.Web.Http.Dependencies;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Classes implement this interface can call an extension method to resolve <see cref="ICacheStrategy"/> from <see cref="IDependencyScope" />
    /// </summary>
    public interface ICacheStrategyResolvable
    {
        /// <summary>
        /// The explicit cache strategy type to resolve
        /// </summary>
        Type CacheStrategyType { get; }
    }
}
