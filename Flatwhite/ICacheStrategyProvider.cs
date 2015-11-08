using System.Collections.Generic;
using Castle.DynamicProxy;

namespace Flatwhite
{
    /// <summary>
    /// A provider to resolve <see cref="ICacheStrategy"/>
    /// </summary>
    public interface ICacheStrategyProvider
    {
        /// <summary>
        /// Get cache strategy by invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        ICacheStrategy GetStrategy(IInvocation invocation, IDictionary<string, object> invocationContext);
    }
}