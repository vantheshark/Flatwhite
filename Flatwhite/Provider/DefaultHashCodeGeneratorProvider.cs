using System;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Provide default implementation of <see cref="IHashCodeGeneratorProvider" />
    /// </summary>
    public class DefaultHashCodeGeneratorProvider : IHashCodeGeneratorProvider
    {
        /// <summary>
        /// Initializes DefaultHashCodeGeneratorProvider with default <see cref="IHashCodeGenerator" /> for all objects
        /// </summary>
        public DefaultHashCodeGeneratorProvider()
        {
            Register<object>(new DefaultHashCodeGenerator());
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

            foreach (var key in Global.Cache.HashCodeGeneratorCache.Keys)
            {
                if (key.IsAssignableFrom(type) && !key.ToString().StartsWith("System."))
                {
                    return Global.Cache.HashCodeGeneratorCache[key];
                }
            }


            if (type.IsEnum || typeof(Enum) == type || type.GetMethod("ToString").DeclaringType != typeof(object))
            {
                Global.Cache.HashCodeGeneratorCache[type] = MethodInfoCache.ToStringGenerator;
                return MethodInfoCache.ToStringGenerator;
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