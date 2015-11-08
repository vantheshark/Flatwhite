using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

namespace Flatwhite
{
    /// <summary>
    /// A class which defined a method to determine whether should stop on an invocation
    /// </summary>
    public abstract class BaseInterceptor
    {
        /// <summary>
        /// Determine whether should stop on an invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        protected virtual bool CanIntercept(IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = invocation.Method.GetCustomAttributes(typeof(NoCacheAttribute), true);
            return !att.Any();
        }
    }
}