using System.Collections.Generic;

namespace Flatwhite.Provider
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
        ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext);
    }
}