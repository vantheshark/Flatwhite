using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Flatwhite.Strategy
{
    /*
        builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .CacheWithStrategy(CacheStrategies.ForService<IUserService>()
                                                  .ForMember(x => x.GetById(Arg.Any<int>()))
                                                  .Duration(1000)
                                                  .VaryByParam("userId")
                                                  .VaryByCustom("threadId")

                                                  .ForMember(x => x.GetByEmail(Arg.Any<string>()))
                                                  .Duration(1000)
                                                  .VaryByParam("email")
                                                  .VaryByCustom("threadId")
                );
    */

    /// <summary>
    /// A strategy to cache on only selected methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TCacheAttribute"></typeparam>
    public class CacheSelectedMethodsInvocationStrategy<T, TCacheAttribute> : DefaultCacheStrategy, IMethodCacheStrategy<T>
        where T : class 
        where TCacheAttribute : OutputCacheAttribute, new()
    {
        private ExpressionSetting<T, TCacheAttribute> _currentExpression;
        private readonly List<ExpressionSetting<T, TCacheAttribute>> _expressions;

        /// <summary>
        /// The setting for a specific invocation
        /// </summary>
        internal CacheSelectedMethodsInvocationStrategy()
        {
            _expressions = new List<ExpressionSetting<T, TCacheAttribute>>();
        }
        /// <summary>
        /// Specify the member for the output cache
        /// </summary>
        /// <param name="functionExpression"></param>
        /// <returns></returns>
        public IMethodCacheRuleBuilder<T> ForMember(Expression<Func<T, object>> functionExpression)
        {
            var cacheAttribute = new TCacheAttribute();
            cacheAttribute.SetCacheStrategy(this);

            var expression = new ExpressionSetting<T, TCacheAttribute>(cacheAttribute)
            {
                Expression = functionExpression,
            };

            _expressions.Add(expression);
            _currentExpression = expression;
            return this;
        }

        /// <summary>
        /// Set the cache duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> Duration(uint duration)
        {
            _currentExpression.CacheAttribute.Duration = duration;
            return this;
        }

        /// <summary>
        /// Set StaleWhileRevalidate
        /// </summary>
        /// <param name="staleWhileRevalidate"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> StaleWhileRevalidate(uint staleWhileRevalidate)
        {
            _currentExpression.CacheAttribute.StaleWhileRevalidate = staleWhileRevalidate;
            return this;
        }

        /// <summary>
        /// Set vary by param for the method
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> VaryByParam(string param)
        {
            _currentExpression.CacheAttribute.VaryByParam = param;
            return this;
        }

        /// <summary>
        /// Set var by custom for the method
        /// </summary>
        /// <param name="custom"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> VaryByCustom(string custom)
        {
            _currentExpression.CacheAttribute.VaryByCustom = custom;
            return this;
        }

        /// <summary>
        /// Set cache store id
        /// </summary>
        /// <param name="cacheStoreId"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> WithCacheStore(uint cacheStoreId)
        {
            _currentExpression.CacheAttribute.CacheStoreId = (int)cacheStoreId;
            return this;
        }

        /// <summary>
        /// Set the cache store _type
        /// </summary>
        /// <param name="cacheStoreType"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> WithCacheStoreType(Type cacheStoreType)
        {
            _currentExpression.CacheAttribute.CacheStoreType = cacheStoreType;
            return this;
        }


        /// <summary>
        /// Set revalidation key format
        /// </summary>
        /// <param name="revalidateKeyFormat"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> WithRevalidateKeyFormat(string revalidateKeyFormat)
        {
            _currentExpression.CacheAttribute.RevalidateKeyFormat = revalidateKeyFormat;
            return this;
        }

        /// <summary>
        /// Set the change monitors factory that will create the new change monitors when new cache entry is created
        /// </summary>
        /// <param name="changeMonitorFactory"></param>
        /// <returns></returns>
        public IMethodCacheStrategy<T> WithChangeMonitors(Func<_IInvocation, IDictionary<string, object>, IEnumerable<IChangeMonitor>> changeMonitorFactory)
        {
            _currentExpression.ChangeMonitorFactory = changeMonitorFactory;
            return this;
        }

        /// <summary>
        /// Get all attributes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<MethodInfo, OutputCacheAttribute>> GetCacheAttributes()
        {
            foreach (var e in _expressions)
            {
                var m = ExpressionHelper.ToMethodInfo(e.Expression);
                yield return new KeyValuePair<MethodInfo, OutputCacheAttribute>(m, e.CacheAttribute);
            }
        }

        /// <summary>
        /// Get change monitors
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public override IEnumerable<IChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var monitors = base.GetChangeMonitors(invocation, invocationContext).ToList();

            foreach (var e in _expressions)
            {
                var m = ExpressionHelper.ToMethodInfo(e.Expression);
                if (m == invocation.Method)
                {
                    monitors.AddRange(e.ChangeMonitorFactory(invocation, invocationContext));
                    break;
                }
            }
            return monitors;
        }
    }
}