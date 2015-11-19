using System;
using System.Collections.Generic;
using System.Linq;
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
        /// List of keys
        /// </summary>
        public List<string> Keys { get; }

        /// <summary>
        /// Initializes a <see cref="RevalidateAttribute" /> with a list of revalidation keys
        /// </summary>
        /// <param name="keys"></param>
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
    }
}
