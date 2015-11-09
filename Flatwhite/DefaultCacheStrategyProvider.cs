using System.Collections.Generic;
using Castle.DynamicProxy;
using Flatwhite.Strategy;

namespace Flatwhite
{
    internal class DefaultCacheStrategyProvider : ICacheStrategyProvider
    {
        public ICacheStrategy GetStrategy(IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return new DefaultCacheStrategy(Global.AttributeProvider, Global.CacheAttributeProvider);
        }
    }
}