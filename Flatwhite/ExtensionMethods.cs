using System.Collections.Generic;

namespace Flatwhite
{
    /// <summary>
    /// Provide some extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Try to get the object from dictionary by key
        /// If not found, return the provided default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The key to get the object</param>
        /// <param name="default">Default value if dictonary doesn't contain key</param>
        /// <returns></returns>
        public static T TryGetByKey<T>(this IDictionary<string, object> dictionary, string key, T @default = default(T))
        {
            if (dictionary == null || key == null || !dictionary.ContainsKey(key))
            {
                return default(T);
            }
            var obj = dictionary[key];
            if (!(obj is T))
            {
                return default(T);
            }
            return (T)obj;
        }
    }
}
