using System;
using System.Collections.Generic;
using System.Reflection;
// ReSharper disable InconsistentNaming

namespace Flatwhite
{
    internal class MethodInfoCache
    {
        public readonly IDictionary<MethodInfo, List<Attribute>> AttributeCache = new Dictionary<MethodInfo, List<Attribute>>();
        public readonly IDictionary<MethodInfo, OutputCacheAttribute> OutputCacheAttributeCache = new Dictionary<MethodInfo, OutputCacheAttribute>();
        public readonly IDictionary<MethodInfo, Tuple<string[], string[]>> VaryParamsCache = new Dictionary<MethodInfo, Tuple<string[], string[]>>();
        public readonly IDictionary<MethodInfo, bool> InterceptableCache = new Dictionary<MethodInfo, bool>();
        public readonly IDictionary<Type, IHashCodeGenerator> HashCodeGeneratorCache = new Dictionary<Type, IHashCodeGenerator>();
    }
}
