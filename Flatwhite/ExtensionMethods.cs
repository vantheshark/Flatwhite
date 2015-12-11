using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

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

        internal static void CheckMethodForCacheSupported(this MethodInfo method, out bool isAsync)
        {
            if (method.ReturnType == typeof(void))
            {
                throw new NotSupportedException("void method is not supported");
            }
            isAsync = typeof(Task).IsAssignableFrom(method.ReturnType);

            if (isAsync && (!method.ReturnType.IsGenericType || method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>)))
            {
                throw new NotSupportedException("async void method is not supported");
            }

            if (isAsync)
            {
                var args = method.ReturnType.GetGenericArguments();
                if (args.Length == 1 && typeof (Task).IsAssignableFrom(args[0]))
                {
                    throw new NotSupportedException("method with return type \"Task<Task>\" does not supported");
                }
            }
        }

        /// <summary>
        /// Try to get a result of a Task when we don't know it's generic argument type
        /// </summary>
        /// <param name="unknownGenericArgumentTypeTaskWithResult"></param>
        /// <returns></returns>
        internal static async Task<object> TryGetTaskResult(this Task unknownGenericArgumentTypeTaskWithResult)
        {
            await unknownGenericArgumentTypeTaskWithResult;
            dynamic taskWithResult = unknownGenericArgumentTypeTaskWithResult;
            return taskWithResult.Result;
        }
    }
}
