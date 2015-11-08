using System;
using System.Linq.Expressions;

namespace Flatwhite.Strategy
{
    /// <summary>
    /// The rule builder for a type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypeCacheRuleBuilder<T> where T : class
    {
        /// <summary>
        /// Create a cache strategy for a member
        /// </summary>
        /// <param name="functionExpression"></param>
        /// <returns></returns>
        IMethodCacheRuleBuilder<T> ForMember(Expression<Func<T, object>> functionExpression);
    }
}