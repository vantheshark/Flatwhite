using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// The member cache rule builder for a memthod on type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMethodCacheRuleBuilder<T> : ITypeCacheRuleBuilder<T> where T : class
    {
        /// <summary>
        /// Set duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> Duration(int duration);
        /// <summary>
        /// Set vảy by param
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> VaryByParam(string param);
        /// <summary>
        /// Set vary by custom
        /// </summary>
        /// <param name="custom"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> VaryByCustom(string custom);

        /// <summary>
        /// Set the change monitors factory that will create the new change monitors when new cache entry is created
        /// https://msdn.microsoft.com/en-us/library/system.runtime.caching.changemonitor(v=vs.110).aspx
        /// </summary>
        /// <param name="changeMonitorFactory"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> WithChangeMonitors(Func<IInvocation, IDictionary<string, object>, IEnumerable<ChangeMonitor>> changeMonitorFactory);
    }
}