using System.Collections.Generic;
using Flatwhite.Strategy;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A <see cref="ICacheStrategy"/> implementation for web api
    /// </summary>
    public class WebApiCacheStrategy : DefaultCacheStrategy
    {
        /// <summary>
        /// Should return true as WebApi doesn't use Dynamic proxy
        /// </summary>
        /// <returns></returns>
        protected override bool CanCacheNoneVirtualOrFinalMethods()
        {
            return true;
        }

        /// <summary>
        /// WebAPI should use AsyncCacheStore instead
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public override ICacheStore GetCacheStore(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            throw new System.NotSupportedException();
        }
    }
}
