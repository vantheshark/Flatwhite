using System;
using System.Collections.Generic;

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
        /// Set StaleWhileRevalidate
        /// </summary>
        /// <param name="staleWhileRevalidate"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> StaleWhileRevalidate(int staleWhileRevalidate);

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
        /// Set cache store id
        /// </summary>
        /// <param name="cacheStoreId"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> WithCacheStore(uint cacheStoreId);

        /// <summary>
        /// Set the cache store type
        /// </summary>
        /// <param name="cacheStoreType"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> WithCacheStoreType(Type cacheStoreType);

        /// <summary>
        /// Set revalidation key
        /// </summary>
        /// <param name="revalidationKey"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> WithRevalidationKey(string revalidationKey);

        /// <summary>
        /// Set the change monitors factory that will create the new change monitors when new cache entry is created
        /// https://msdn.microsoft.com/en-us/library/system.runtime.caching.changemonitor(v=vs.110).aspx
        /// </summary>
        /// <param name="changeMonitorFactory"></param>
        /// <returns></returns>
        IMethodCacheStrategy<T> WithChangeMonitors(Func<_IInvocation, IDictionary<string, object>, IEnumerable<IChangeMonitor>> changeMonitorFactory);
    }
}