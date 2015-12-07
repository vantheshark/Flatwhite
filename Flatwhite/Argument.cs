using System.Diagnostics.CodeAnalysis;

namespace Flatwhite
{
    /// <summary>
    /// A class which can provide any argument for caching stuff
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Argument
    {
        /// <summary>
        /// Match any argument value compatible with type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Any<T>()
        {
            return default(T);
        }
    }
}