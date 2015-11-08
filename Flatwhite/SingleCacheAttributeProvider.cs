using System.Collections.Generic;
using System.Reflection;

namespace Flatwhite
{
    /// <summary>
    /// The provider that will return 1 <see cref="OutputCacheAttribute"/> no matter what MethodInfo and context is
    /// </summary>
    public class SingleCacheAttributeProvider : ICacheAttributeProvider
    {
        /// <summary>
        /// The attribute that will be returned
        /// </summary>
        public OutputCacheAttribute Attribute { get; }

        /// <summary>
        /// Initialize the provider with the provided attribute
        /// </summary>
        /// <param name="attribute"></param>
        public SingleCacheAttributeProvider(OutputCacheAttribute attribute)
        {
            Attribute = attribute;
        }

        /// <summary>
        /// Return the set attribute
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public OutputCacheAttribute GetCacheAttribute(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            return Attribute;
        }
    }
}