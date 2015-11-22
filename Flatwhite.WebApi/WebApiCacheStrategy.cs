using System.Collections.Generic;
using System.Linq;
using Flatwhite.Strategy;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A <see cref="ICacheStrategy"/> implementation for web api
    /// </summary>
    public class WebApiCacheStrategy : DefaultCacheStrategy
    {
        /// <summary>
        /// Create an instance of WebApiCacheStrategy with ServiceActivator
        /// </summary>
        /// <param name="activator"></param>
        public WebApiCacheStrategy(IServiceActivator activator = null) : base(activator)
        {
        }
        /// <summary>
        /// Should always return true as it doesn't need to check the method info
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public override bool CanIntercept(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (!Global.Cache.InterceptableCache.ContainsKey(invocation.Method))
            {
                //https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isvirtual(v=vs.110).aspx
                var possible = invocation.Method.ReturnType != typeof(void);
                if (possible)
                {
                    var atts = Global.AttributeProvider.GetAttributes(invocation.Method, invocationContext);
                    possible = !atts.Any(a => a is NoInterceptAttribute);
                }
                Global.Cache.InterceptableCache[invocation.Method] = possible;
            }

            return Global.Cache.InterceptableCache[invocation.Method];
        }

        /// <summary>
        /// Get <see cref="ICacheStore" /> for current invocation and context
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
