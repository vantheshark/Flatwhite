using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flatwhite.Provider
{
    /// <summary>
    /// A dynamic attribute provider to provide extra attributes to method without having to decorate the attributes on class or method
    /// </summary>
    public class DynamicAttributeProvider : IAttributeProvider
    {
        private readonly IAttributeProvider _original;
        private readonly Func<List<Tuple<MethodInfo, Attribute>>> _extraAttributes;

        /// <summary>
        /// Initialize a dynamic attribute provider with extra attributes for selected method info
        /// </summary>
        /// <param name="original"></param>
        /// <param name="extraAttributes"></param>
        public DynamicAttributeProvider(IAttributeProvider original, Func<List<Tuple<MethodInfo, Attribute>>> extraAttributes)
        {
            _original = original;
            _extraAttributes = extraAttributes;
        }

        /// <summary>
        /// Get all attributes, attributes returned by extraAttributes factory will have more priority than 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public IEnumerable<Attribute> GetAttributes(MethodInfo methodInfo, IDictionary<string, object> invocationContext)
        {
            var attributes = _extraAttributes()
                .Where(x => x.Item1 == null || x.Item1 == methodInfo)
                .Select(x => x.Item2)
                .ToList();

            var uniqueTypes = attributes
                .Select(a => a.GetType())
                .Where(type => type.GetCustomAttributes<AttributeUsageAttribute>().Any(x => x.AllowMultiple == false))
                .ToList();

            var originalAttributes = _original.GetAttributes(methodInfo, invocationContext)
                .Where(a => uniqueTypes.All(t => t != a.GetType()));

            return originalAttributes.Union(attributes);
        }
    }
}