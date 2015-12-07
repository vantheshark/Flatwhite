using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Flatwhite.Provider;
using Flatwhite.Strategy;

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
        public ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return invocationContext.ContainsKey(WebApiExtensions.__webApi) 
                ? new WebApiCacheStrategy() 
                : new DefaultCacheStrategy();
        }
    }
}