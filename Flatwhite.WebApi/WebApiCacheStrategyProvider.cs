using System.Collections.Generic;
using Flatwhite.Provider;
using Flatwhite.Strategy;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A provider to return <see cref="DefaultCacheStrategy" /> with different <see cref="ICacheAttributeProvider" /> if the context is webapi request
    /// </summary>
    public class WebApiCacheStrategyProvider : ICacheStrategyProvider
    {
        /// <summary>
        /// Return a <see cref="ICacheStrategy" /> if the request is webApi request
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return invocationContext.ContainsKey(WebApiExtensions.__webApi) 
                ? (ICacheStrategy)new WebApiCacheStrategy() 
                : new DefaultCacheStrategy(Global.AttributeProvider, Global.CacheAttributeProvider);
        }
    }
}