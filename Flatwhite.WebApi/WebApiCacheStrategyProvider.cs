using System.Collections.Generic;
using Flatwhite.Strategy;

namespace Flatwhite.WebApi
{
    public class WebApiCacheStrategyProvider : ICacheStrategyProvider
    {
        public ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            if (invocationContext.ContainsKey("__webApi"))
                return new DefaultCacheStrategy(Global.AttributeProvider, new WebApiCacheAttributeProvider());
            else
                return new DefaultCacheStrategy(Global.AttributeProvider, Global.CacheAttributeProvider);
        }
    }
}