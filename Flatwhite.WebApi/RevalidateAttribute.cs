using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Flatwhite.Provider;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Use this attribute to decorate on a method where you want to revalidate a specific cache entry after a method is invoked
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RevalidateAttribute : ActionFilterAttribute
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
        /// Revalidate caches after call method
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <returns></returns>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.ActionContext.Response != null && actionExecutedContext.ActionContext.Response.IsSuccessStatusCode)
            {
                var invocation = GetInvocation(actionExecutedContext.ActionContext);
                var revalidatedKeys = KeyFormats.Select(k => CacheKeyProvider.GetRevalidateKey(invocation, k)).ToList();
                Global.RevalidateCaches(revalidatedKeys);
            }
        }

        /// <summary>
        /// Revalidate caches after call method
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.ActionContext.Response != null && actionExecutedContext.ActionContext.Response.IsSuccessStatusCode)
            {
                var invocation = GetInvocation(actionExecutedContext.ActionContext);
                var revalidatedKeys = KeyFormats.Select(k => CacheKeyProvider.GetRevalidateKey(invocation, k)).ToList();

                return Global.RevalidateCachesAsync(revalidatedKeys);
            }
            return TaskHelpers.DefaultCompleted;
        }

        /// <summary>
        /// Get <see cref="_IInvocation" /> from <see cref="HttpActionContext" />
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual _IInvocation GetInvocation(HttpActionContext actionContext)
        {
            return new WebApiInvocation(actionContext);
        }

        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider => Global.CacheKeyProvider;
    }
}
