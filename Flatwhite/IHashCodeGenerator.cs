using System;
using Flatwhite.Provider;

namespace Flatwhite
{
    /// <summary>
    /// Provides an abstraction for serialize a custom value from invocationContext when building the cache key by <see cref="ICacheKeyProvider"></see>
    /// implementation can be registered via the <see cref="IHashCodeGenerator"/>.
    /// </summary>
    public interface IHashCodeGenerator
    {
        /// <summary>
        /// Return the code which will be use as a part of cache key
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string GetCode(object value);
    }

    /// <summary>
    /// Provie default implementation of <see cref="IHashCodeGenerator"/>.
    /// </summary>
    public class DefaultHashCodeGenerator : IHashCodeGenerator
    {
        /// <summary>
        /// Return the code which will be use as a part of cache key
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetCode(object value)
        {
            return value?.GetHashCode().ToString() ?? "null";
        }
    }

    internal class ToStringHashCodeGenerator : IHashCodeGenerator
    {
        /// <summary>
        /// Return the code which will be use as a part of cache key
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetCode(object value)
        {
            return value?.ToString() ?? "null";
        }
    }
}