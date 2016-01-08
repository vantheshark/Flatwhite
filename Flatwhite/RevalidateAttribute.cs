using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Flatwhite.Provider;

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
        public List<string> KeyFormats { get; }

        /// <summary>
        /// Initializes a <see cref="RevalidateAttribute" /> with a list of revalidation keys
        /// </summary>
        /// <param name="keyFormats">List of "revalidation key format" to notify the cache store. They are not the cache keys</param>
        public RevalidateAttribute(params string[] keyFormats)
        {
            KeyFormats = keyFormats.ToList();
        }

        /// <summary>
        /// Clear the cache with the provided revalidation keys
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        /// <returns></returns>
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            var revalidatedKeys = KeyFormats.Select(k => CacheKeyProvider.GetRevalidateKey(methodExecutedContext.Invocation, k)).ToList();
            Global.RevalidateCaches(revalidatedKeys);
        }

        /// <summary>
        /// Revalidate cache async
        /// </summary>
        /// <param name="methodExecutedContext"></param>
        /// <returns></returns>
        public override Task OnMethodExecutedAsync(MethodExecutedContext methodExecutedContext)
        {
            var revalidateKeys = KeyFormats.Select(k => CacheKeyProvider.GetRevalidateKey(methodExecutedContext.Invocation, k)).ToList();
            return Global.RevalidateCachesAsync(revalidateKeys);
        }

        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider => Global.CacheKeyProvider;
    }
}