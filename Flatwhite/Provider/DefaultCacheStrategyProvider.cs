using System.Collections.Generic;
using Flatwhite.Strategy;

namespace Flatwhite.Provider
{
    internal class DefaultCacheStrategyProvider : ICacheStrategyProvider
    {
        public ICacheStrategy GetStrategy(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return new DefaultCacheStrategy(Global.AttributeProvider, Global.CacheAttributeProvider);
        }
    }
}