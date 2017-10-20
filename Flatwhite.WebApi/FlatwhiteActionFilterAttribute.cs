using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Base filter attribute with some method to Get infocation and context
    /// </summary>
    public abstract class FlatwhiteActionFilterAttribute : ActionFilterAttribute
    {
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
        /// Get context data from <see cref="HttpActionContext" />
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> GetInvocationContext(HttpActionContext actionContext)
        {
            var provider = new WebApiContextProvider(actionContext);
            var context = provider.GetContext();
            context[Global.__flatwhite_outputcache_attribute] = this;
            return context;
        }
    }
}