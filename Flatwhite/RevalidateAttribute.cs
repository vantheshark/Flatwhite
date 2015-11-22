using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method where you want to revalidate a specific cache entry after a method is invoked
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RevalidateAttribute : MethodFilterAttribute
    {
        /// <summary>
        /// List of "revalidation keys" to notify the cache store. They are not neccessary the cache key
        /// </summary>
        public List<string> Keys { get; }

        /// <summary>
        /// Initializes a <see cref="RevalidateAttribute" /> with a list of revalidation keys
        /// </summary>
        /// <param name="keys">List of "revalidation keys" to notify the cache store. They are not neccessary the cache key</param>
        public RevalidateAttribute(params string[] keys)
        {
            Keys = keys.ToList();
        }

        /// <summary>
        /// Clear the cache with the provided revalidation keys
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        /// <returns></returns>
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            Global.RevalidateCaches(Keys);
        }

        /// <summary>
        /// Revalidate cache async
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        /// <returns></returns>
        public override Task OnMethodExecutedAsync(MethodExecutedContext methodExecutedContext)
        {
            return Global.RevalidateCachesAsync(Keys);
        }
    }
}