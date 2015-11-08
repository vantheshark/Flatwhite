using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Caching;
using Castle.DynamicProxy;

namespace Flatwhite
{
    /// <summary>
    /// Output cache settings for a method expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TCacheAttribute"></typeparam>
    public class ExpressionSetting<T, TCacheAttribute>
        where T : class
        where TCacheAttribute : OutputCacheAttribute, new()
    {
        /// <summary>
        /// Init an instance of ExpressionSetting with empty ChangeMonitor list and default cache attribute
        /// </summary>
        public ExpressionSetting()
        {
            ChangeMonitorFactory = (i, context) => new ChangeMonitor[0];
            CacheAttribute = new TCacheAttribute();
        }
        /// <summary>
        /// The expression represents the method invocation
        /// </summary>
        public Expression<Func<T, object>> Expression { get; set; }

        /// <summary>
        /// The cache attribute for the current expression
        /// </summary>
        public TCacheAttribute CacheAttribute { get; set; }

        /// <summary>
        /// The change monitors factory configured for current expression
        /// <para>These change monitors instances will be created and assign to the cache policy for the new cache</para>
        /// </summary>
        public Func<IInvocation, IDictionary<string, object>, IEnumerable<ChangeMonitor>> ChangeMonitorFactory { get; set; }
    }
}