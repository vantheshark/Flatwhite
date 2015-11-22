using System;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable InconsistentNaming

namespace Flatwhite
{
    internal class MethodInfoCache
    {
        internal static readonly ToStringHashCodeGenerator ToStringGenerator = new ToStringHashCodeGenerator();
        /// <summary>
        /// Use <see cref="ToStringHashCodeGenerator" /> for all primitive types
        /// Use <see cref="DefaultHashCodeGenerator" /> for unregistered types
        /// </summary>
        public MethodInfoCache()
        {
            var primitiveTypes = new List<Type>
            {
                typeof (short),
                typeof (ushort),
                typeof (int),
                typeof (uint),
                typeof (long),
                typeof (ulong),

                typeof (char),
                typeof (byte),
                typeof (string),

                typeof (decimal),
                typeof (float),
                typeof (double),

                typeof (DateTime),
                typeof (Guid),

            };
            var nullable = typeof(Nullable<>);
            primitiveTypes.ForEach(t =>
            {
                HashCodeGeneratorCache[t] = ToStringGenerator;
                if (t != typeof(string))
                {
                    HashCodeGeneratorCache[nullable.MakeGenericType(t)] = ToStringGenerator;
                }
            });
        }

        public readonly IDictionary<MethodInfo, List<Attribute>> AttributeCache = new Dictionary<MethodInfo, List<Attribute>>();
        public readonly IDictionary<MethodInfo, OutputCacheAttribute> OutputCacheAttributeCache = new Dictionary<MethodInfo, OutputCacheAttribute>();
        public readonly IDictionary<MethodInfo, Tuple<string[], string[]>> VaryParamsCache = new Dictionary<MethodInfo, Tuple<string[], string[]>>();
        public readonly IDictionary<MethodInfo, bool> InterceptableCache = new Dictionary<MethodInfo, bool>();
        public readonly IDictionary<Type, IHashCodeGenerator> HashCodeGeneratorCache = new Dictionary<Type, IHashCodeGenerator>();
        public readonly Dictionary<string, Phoenix> Phoenix = new Dictionary<string, Phoenix>();
    }
}
