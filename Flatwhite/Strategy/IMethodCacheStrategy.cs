namespace Flatwhite.Strategy
{
    /// <summary>
    /// A method cache strategy created for selected member
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMethodCacheStrategy<T> : IMethodCacheRuleBuilder<T>, IDynamicCacheStrategy where T : class
    {
    }
}