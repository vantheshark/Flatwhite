using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace Flatwhite
{
    /// <summary>
    /// Output cache settings for a method expression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TCacheAttribute"></typeparam>
    public class ExpressionSetting<T, TCacheAttribute>
        where T : class
        where TCacheAttribute : OutputCacheAttribute
    {
        /// <summary>
        /// Init an instance of ExpressionSetting with empty ChangeMonitor list and default cache attribute
        /// </summary>
        public ExpressionSetting(TCacheAttribute cacheAttribute)
        {
            CacheAttribute = cacheAttribute;
            ChangeMonitorFactory = (i, context) => new IChangeMonitor[0];
        }

        /// <summary>
        /// The expression represents the method invocation
        /// </summary>
        public Expression<Func<T, object>> Expression { get; set; }

        /// <summary>
        /// The cache attribute for the current expression
        /// </summary>
        public TCacheAttribute CacheAttribute { get; }

        /// <summary>
        /// The change monitors factory configured for current expression
        /// <para>These change monitors instances will be created and assign to the cache policy for the new cache</para>
        /// </summary>
        public Func<_IInvocation, IDictionary<string, object>, IEnumerable<IChangeMonitor>> ChangeMonitorFactory { get; set; }
    }
}