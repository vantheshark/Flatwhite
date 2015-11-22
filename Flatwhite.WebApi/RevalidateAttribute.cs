using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

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
        /// Revalidate caches after call method
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <returns></returns>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.ActionContext.Response != null && actionExecutedContext.ActionContext.Response.IsSuccessStatusCode)
            {
                Global.RevalidateCaches(Keys);
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
                return Global.RevalidateCachesAsync(Keys);
            }
            return TaskHelpers.DefaultCompleted;
        }
    }
}
