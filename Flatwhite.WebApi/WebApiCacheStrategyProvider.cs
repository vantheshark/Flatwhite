using Flatwhite.Provider;
using Flatwhite.Strategy;
using System.Collections.Generic;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A provider to return <see cref="WebApiCacheStrategy" /> for WebApi request
    /// </summary>
    public class WebApiCacheStrategyProvider : ICacheStrategyProvider
    {
        /// <summary>
        /// Return a <see cref="ICacheStrategy" /> if the request is webApi request
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return invocationContext.ContainsKey(WebApiExtensions.__webApi) 
                ? new WebApiCacheStrategy() 
                : new DefaultCacheStrategy();
        }
    }
}