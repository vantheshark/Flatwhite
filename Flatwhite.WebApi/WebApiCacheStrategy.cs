using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using Flatwhite.Provider;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// A <see cref="ICacheStrategy"/> implementation for web api
    /// </summary>
    public class WebApiCacheStrategy : ICacheStrategy
    {
        internal WebApiCacheStrategy() : this(new WebApiCacheAttributeProvider())
        {
        }

        /// <summary>
        /// The cache attribute provider
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected readonly ICacheAttributeProvider _cacheAttributeProvider;

        /// <summary>
        /// Initializes default cache strategy with a <see cref="ICacheAttributeProvider"/>
        /// </summary>
        /// <param name="cacheAttributeProvider"></param>
        public WebApiCacheStrategy(ICacheAttributeProvider cacheAttributeProvider)
        {
            _cacheAttributeProvider = cacheAttributeProvider;
            CacheKeyProvider = new DefaultCacheKeyProvider(cacheAttributeProvider, Global.HashCodeGeneratorProvider);
        }

        /// <summary>
        /// Should always return true as it doesn't need to check the method info
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual bool CanIntercept(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return true;
        }

        /// <summary>
        /// Get cache time from <see cref="OutputCacheAttribute"/>
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual int GetCacheTime(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            return att?.Duration ?? 0;
        }

        /// <summary>
        /// Get cache store id for current invocation and context
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual uint GetCacheStoreId(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            return att?.CacheStoreId ?? 0;
        }

        /// <summary>
        /// Get empty list change monitor
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ChangeMonitor> GetChangeMonitors(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var att = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
            if (string.IsNullOrWhiteSpace(att?.RevalidationKey))
            {
                yield break;
            }
            yield return new FlatwhiteCacheEntryChangeMonitor(att.RevalidationKey);
            //TODO: Don't remove if there is stale while validation settings
        }

        /// <summary>
        /// Default cache key provider
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ICacheKeyProvider CacheKeyProvider { get; }

    }
}
