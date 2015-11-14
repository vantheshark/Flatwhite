using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using Flatwhite.Provider;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// https://www.mnot.net/blog/2007/12/12/stale
    /// https://tools.ietf.org/html/rfc5861
    /// https://devcenter.heroku.com/articles/increasing-application-performance-with-http-cache-headers
    /// </summary>
    public class OutputCacheAttribute : ActionFilterAttribute
    {
        #region -- Cache params --
        /// <summary>
        /// The custom type of <see cref="ICacheStrategy" /> to use. If not provided, the default strategy from Global will be used
        /// </summary>
        public Type CacheStrategyType { get; set; }

        /// <summary>
        /// The custom type of <see cref="ICacheStore" /> to use. If not provided, the default strategy from Global will be used
        /// </summary>
        public Type CacheStoreType { get; set; }

        /// <summary>
        /// The unique number id of the cache store when registered against the <see cref="ICacheStoreProvider" />
        /// </summary>
        public uint CacheStoreId { get; set; }

        /// <summary>
        /// Gets or sets the cache duration, in seconds.
        /// <para>Also set Cache-Control: max-age=*seconds* to the message response header.</para>
        /// </summary>
        public uint MaxAge { get; set; }
        
        /// <summary>
        /// A semicolon-separated list of strings that correspond to to parameter values
        /// </summary>
        public string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the VaryByCustom value which facilitate different cache rules by custom key in invocationContext
        /// </summary>
        public string VaryByCustom { get; set; }

        /// <summary>
        /// The VaryByHeader is a semicolon-delimited set of headers used to vary the cached output. These are HTTP headers associated with the request
        /// </summary>
        public string VaryByHeader { get; set; }

        /// <summary>
        /// Set Cache-Control: must-revalidate to the message response header.
        /// <para>Whether the origin server require revalidation of a cache entry on any subsequent use when the cache entry becomes stale.</para>
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.4</para> 
        /// </summary>
        public bool MustRevalidate { get; set; }

        /// <summary>
        /// Set Cache-Control: proxy-revalidate to the message response header.
        /// <para>The proxy-revalidate directive has the same meaning as the must- revalidate directive, except that it does not apply to non-shared user agent caches.</para>
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.4</para>
        /// </summary>
        public bool ProxyRevalidate { get; set; }

        /// <summary>
        /// Set Cache-Control: no-cache to the message response header. IE uses no-cache, and Firefox uses no-store.
        /// <para>If the no-cache directive does not specify a field-name, then a cache MUST NOT use the response to satisfy a subsequent request without successful revalidation with the origin server. This allows an origin server to prevent caching even by caches that have been configured to return stale responses to client requests.</para>
        /// <para>If the no-cache directive does specify one or more field-names, then a cache MAY use the response to satisfy a subsequent request, subject to any other restrictions on caching.However, the specified field-name(s) MUST NOT be sent in the response to a subsequent request without successful revalidation with the origin server.This allows an origin server to prevent the re-use of certain header fields in a response, while still allowing caching of the rest of the response.</para>
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.1</para>
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// Set Cache-Control: no-store to the message response header. IE uses no-cache, and Firefox uses no-store.
        /// <para>A cache (browser cache, proxies) MUST NOT store any part of either this response or the request that elicited it. This directive applies to both non-shared and shared caches.</para>
        /// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.2
        /// </summary>
        public bool NoStore { get; set; }

        /// <summary>
        /// Set Cache-Control:no-transform to the message response header.
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.5</para>
        /// </summary>
        public bool NoTransform { get; set; }

        /// <summary>
        /// Set Cache-Control: private to the message response header.
        /// <para>Indicates that all or part of the response message is intended for a single user and MUST NOT be cached by a shared cache.</para>
        /// <para>This allows an origin server to state that the specified parts of the response are intended for only one user and are not a valid response for requests by other users.</para>
        /// <para>A private (non-shared) cache MAY cache the response.</para>
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.1</para>
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Set Cache-Control: public to the message response header.
        /// <para>Indicates that the response MAY be cached by any cache, even if it would normally be non-cacheable or cacheable only within a non- shared cache. (See also Authorization, section 14.8, for additional details.)</para>
        /// <para>http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.9.1</para>
        /// </summary>
        public bool Public { get; set; }

        /// <summary>
        /// Set Cache-Control: max-age=*seconds*, stale-while-revalidate=*seconds* to the response message
        /// <para>This should be used with <see cref="MaxAge" /> to indicates that caches MAY serve the response in which it appears after it becomes stale, up to the indicated number of seconds</para>
        /// <para>https://tools.ietf.org/html/rfc5861</para> 
        /// </summary>
        public uint StaleWhileRevalidate { get; set; }

        /// <summary>
        /// Set Cache-Control: max-age=*seconds*, stale-if-error=*seconds* to the response message
        /// <para>This should be used with <see cref="MaxAge" /> to indicates that caches may be used if an error is encountered after becoming stale for an additional indicated number of seconds</para>
        /// https://tools.ietf.org/html/rfc5861#4.1
        /// </summary>
        public uint StaleIfError { get; set; }
        #endregion 

        /// <summary>
        /// Get <see cref="IAsyncCacheStore" /> from <see cref="IDependencyScope" />
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected virtual IAsyncCacheStore GetAsyncCacheStore(IDependencyScope scope)
        {
            if (CacheStoreId > 0)
            {
                return Global.CacheStoreProvider.GetAsyncCacheStore(CacheStoreId);
            }
            
            if (CacheStoreType != null)
            {
                return scope.GetService(CacheStoreType) as IAsyncCacheStore ?? Global.CacheStoreProvider.GetAsyncCacheStore();
            }

            return Global.CacheStoreProvider.GetAsyncCacheStore();
        }

        /// <summary>
        /// Get <see cref="ICacheStrategy" /> from <see cref="IDependencyScope" />
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        protected virtual ICacheStrategy GetCacheStrategy(IDependencyScope scope, _IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var strategy = CacheStrategyType != null ? scope.GetService(CacheStrategyType) as ICacheStrategy : null;
            if (strategy == null)
            {
                var strategyProvider = scope.GetService(typeof (ICacheStrategyProvider)) as ICacheStrategyProvider ?? Global.CacheStrategyProvider;
                strategy = strategyProvider.GetStrategy(invocation, invocationContext);
            }
            if (strategy == null) throw new Exception("Cannot find caching strategy for this requiest");
            return strategy;
        }

        /// <summary>
        /// Get <see cref="ICacheResponseBuilder" /> from <see cref="IDependencyScope" />
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected virtual ICacheResponseBuilder GetCacheResponseBuilder(IDependencyScope scope)
        {
            return scope.GetService(typeof(ICacheResponseBuilder)) as ICacheResponseBuilder ??
                   new CacheResponseBuilder();
        }
        
        /// <summary>
        /// Check CacheControl request, get CacheItem, build response and return if cache available
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="cancellationToken"></param>
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (NoCache || NoStore || MaxAge == 0 || Private)
            {
                return;
            }
            
            // Check CacheControl request, get CacheItem, build response and return
            var cacheControl = actionContext.Request.Headers.CacheControl;

            var contextProvider = new WebApiContextProvider(actionContext);
            var invocation = new WebApiInvocation(actionContext);
            var context = contextProvider.GetContext();
            context[typeof (OutputCacheAttribute).Name] = this;

            var scope = actionContext.Request.GetDependencyScope();

            var strategy = GetCacheStrategy(scope, invocation, context);
            if (strategy.CanIntercept(invocation, context) || strategy.GetCacheTime(invocation, context) <= 0)
            {
                var cachekey = strategy.CacheKeyProvider.GetCacheKey(invocation, context);
                var hashedKey = HashCacheKey(cachekey);
                var cacheStore = GetAsyncCacheStore(scope);
                var hashKey = $"fw-{cacheStore.StoreId}-{hashedKey}";

                var cacheItem = await cacheStore.GetAsync(hashKey) as CacheItem;
                if (cacheItem != null)
                {
                    var builder = GetCacheResponseBuilder(scope);
                    var response = builder?.GetResponse(cacheControl, cacheItem, actionContext.Request);

                    if (response != null)
                    {
                        response.Content = new ByteArrayContent(cacheItem.Content);
                        response.Headers.ETag = new EntityTagHeaderValue($"\"{hashKey}\"");

                        actionContext.Response = response;
                    }
                }
                else if (cacheControl != null && cacheControl.OnlyIfCached)
                {
                    var response = new HttpResponseMessage { StatusCode = HttpStatusCode.GatewayTimeout };
                    response.Headers.Add("X-Flatwhite-Message", "no cache available");
                    actionContext.Response = response;
                }
            }
        }

        /// <summary>
        /// Store the response to cache store, add CacheControl and Etag to response
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.ActionContext.Response == null ||
               !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode) return;

            //NOTE: Apply all CacheControl header
            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, actionExecutedContext.Request.Properties);

            if (Private /*We should not deal with private data*/ || NoCache || NoStore || MaxAge == 0 || 
                actionExecutedContext.Request.Properties.Any( prop => prop.Key.StartsWith(CacheItem.DONT_CACHE_PREFIX)))
            {
                return;
            }

            var contextProvider = new WebApiContextProvider(actionExecutedContext.ActionContext);
            var invocation = new WebApiInvocation(actionExecutedContext.ActionContext);
            var context = contextProvider.GetContext();
            context[typeof(OutputCacheAttribute).Name] = this;

            var scope = actionExecutedContext.ActionContext.Request.GetDependencyScope();

            var strategy = GetCacheStrategy(scope, invocation, context);
            if (!strategy.CanIntercept(invocation, context) || strategy.GetCacheTime(invocation, context) <= 0)
            {
                return;
            }
            
            var responseContent = actionExecutedContext.Response.Content;

            if (responseContent != null)
            {
                var cacheItem = new CacheItem(this)
                {
                    Content = await responseContent.ReadAsByteArrayAsync().ConfigureAwait(false)
                };
                
                var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddSeconds(MaxAge + Math.Max(StaleWhileRevalidate, StaleIfError))};
                //TODO: Refresh cache when it starts to be stale
                var cachekey = strategy.CacheKeyProvider.GetCacheKey(invocation, context);
                var hashedKey = HashCacheKey(cachekey);

                var changeMonitors = strategy.GetChangeMonitors(invocation, context, hashedKey);
                foreach (var mon in changeMonitors)
                {
                    policy.ChangeMonitors.Add(mon);
                }
                

                var cacheStore = GetAsyncCacheStore(scope);
                var cacheKey = $"fw-{cacheStore.StoreId}-{hashedKey}";
                actionExecutedContext.Response.Headers.ETag = new EntityTagHeaderValue($"\"{cacheKey}\"");
                
                await cacheStore.SetAsync(cacheKey, cacheItem, policy).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Apply the CacheControl header to response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="requestProperties"></param>
        protected virtual void ApplyCacheHeaders(HttpResponseMessage response, IDictionary<string, object> requestProperties)
        {
            var cachecontrol = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(MaxAge),
                MustRevalidate = MustRevalidate,
                ProxyRevalidate = ProxyRevalidate,
                Private = Private,
                Public = Public,
                NoStore = NoStore,
                NoCache = NoCache,
                MaxStale = Math.Max(StaleIfError, StaleWhileRevalidate) > 0,
                MaxStaleLimit = TimeSpan.FromSeconds(Math.Max(StaleIfError, StaleWhileRevalidate)),
                NoTransform = NoTransform
            };

            response.Headers.CacheControl = cachecontrol;

            if (MaxAge > 0 || MustRevalidate || Private)
            {
                response.Headers.CacheControl.MaxAge = TimeSpan.FromSeconds(MaxAge);
            }

            if (NoCache)
            {
                response.Headers.Add("Pragma", "no-cache");
            }

            foreach (var prop in requestProperties)
            {
                if (prop.Key.StartsWith(CacheItem.FLATWHITE_PREFIX))
                {
                    response.Headers.Add("X-Flatwhite-" + prop.Key.Replace(CacheItem.DONT_CACHE_PREFIX, "")
                                                                  .Replace(CacheItem.FLATWHITE_PREFIX, ""), 
                                        (string)prop.Value);
                }
            }
        }

        /// <summary>
        /// Make a hash string of the original senstivive cacheKey
        /// </summary>
        /// <param name="originalCacheKey"></param>
        /// <returns></returns>
        protected virtual string HashCacheKey(string originalCacheKey)
        {
            using (var md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(Encoding.ASCII.GetBytes(originalCacheKey)).ToHex();
            }
        }
    }
}
