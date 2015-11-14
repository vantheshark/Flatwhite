using System;
using System.Collections.Generic;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Provide default implementation of <see cref="IHashCodeGeneratorProvider" />
    /// </summary>
    public class DefaultHashCodeGeneratorProvider : IHashCodeGeneratorProvider
    {
        private static readonly ToStringHashCodeGenerator _toStringGenerator = new ToStringHashCodeGenerator();
        /// <summary>
        /// Use <see cref="ToStringHashCodeGenerator" /> for all primitive types
        /// Use <see cref="DefaultHashCodeGenerator" /> for unregistered types
        /// </summary>
        public DefaultHashCodeGeneratorProvider()
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
            var nullable = typeof (Nullable<>);
            primitiveTypes.ForEach(t =>
            {
                Global.Cache.HashCodeGeneratorCache[t] = _toStringGenerator;
                if (t != typeof (string))
                {
                    Global.Cache.HashCodeGeneratorCache[nullable.MakeGenericType(t)] = _toStringGenerator;
                }
            });
        }

        /// <summary>
        /// Return <see cref="IHashCodeGenerator" /> for type.
        /// <para>if Type implemented ToString(), the type will be registered and <see cref="ToStringHashCodeGenerator" /> will be used </para>
        /// </summary>
        /// <returns></returns>
        public IHashCodeGenerator GetForType(Type type)
        {
            if (Global.Cache.HashCodeGeneratorCache.ContainsKey(type))
            {
                return Global.Cache.HashCodeGeneratorCache[type];
            }

            if (type.IsEnum || type.GetMethod("ToString").DeclaringType != typeof(object))
            {
                Global.Cache.HashCodeGeneratorCache[type] = _toStringGenerator;
                return _toStringGenerator;
            }

            return new DefaultHashCodeGenerator();
        }

        /// <summary>
        /// Register a custom serializer for T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashCodeGenerator"></param>
        public void Register<T>(IHashCodeGenerator hashCodeGenerator)
        {
            if (hashCodeGenerator == null)
            {
                throw new ArgumentNullException(nameof(hashCodeGenerator));
            }
            Global.Cache.HashCodeGeneratorCache[typeof (T)] = hashCodeGenerator;
        }
    }
}