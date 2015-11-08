namespace Flatwhite.Strategy
{
    /// <summary>
    /// A factory class to provide default <see cref="ICacheStrategy"/>
    /// </summary>
    public static class CacheStrategies 
    {
        /// <summary>
        /// Enable caching for all methods that have output value, default cache time is 1 second
        /// </summary>
        /// <returns></returns>
        public static CacheOutputForAllMethod AllMethods()
        {
            return new CacheOutputForAllMethod(1000);
        }

        /// <summary>
        /// Enable caching on selected methods only
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ITypeCacheRuleBuilder<T> ForService<T>() where T : class
        {
            return new CacheSelectedMethodsInvocationStrategy<T, OutputCacheAttribute>();
        }
    }
}