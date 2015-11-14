using System;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Provides an abstraction for resolving <see cref="IHashCodeGenerator" />
    /// </summary>
    public interface IHashCodeGeneratorProvider
    {
        /// <summary>
        /// Return <see cref="IHashCodeGenerator" />
        /// </summary>
        /// <returns></returns>
        IHashCodeGenerator GetForType(Type type);

        /// <summary>
        /// Register a custom serializer for T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashCodeGenerator"></param>
        void Register<T>(IHashCodeGenerator hashCodeGenerator);
    }
}