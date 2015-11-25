using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
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
    //https://www.mnot.net/blog/2007/12/12/stale
    //https://tools.ietf.org/html/rfc5861
    //https://devcenter.heroku.com/articles/increasing-application-performance-with-http-cache-headers

    /// <summary>
    /// Represents an attribute that is used to mark an WebApi action method whose output will be cached.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class OutputCacheAttribute : ActionFilterAttribute, ICacheSettings
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
        public int CacheStoreId { get; set; }

        /// <summary>
        /// Gets or sets the cache duration, in seconds.
        /// <para>Also set Cache-Control: max-age=*seconds* to the message response header.</para>
        /// </summary>
        public uint MaxAge { get; set; }

        /// <summary>
        /// Set Cache-Control: s-maxage=*seconds* to the message response header.
        /// </summary>
        public uint SMaxAge { get; set; }

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


        /// <summary>
        /// A cache MAY be configured to return stale responses without validation
        /// <para>If set to TRUE, the server will return cache item as soon as the cache item is available and ignore all cache control directives sent from client
        /// such as no-cache, no-store or max-age, max-stale. Warning 110 (Response is stale) will be included in the response header</para>
        /// <para>This may be helpful to guarantee that the endpoint will not revalidate the cache all the time by some one sending request with no-cache header</para>
        /// </summary>
        public bool IgnoreRevalidationRequest { get; set; }

        /// <summary>
        /// A key to used to delete the cache when an method with relevant <see cref="RevalidateAttribute" /> is invoked
        /// </summary>
        public string RevalidationKey { get; set; }

        #endregion

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
            if (strategy == null) throw new Exception("Cannot find caching strategy for this request");
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
            /*
            cache-request-directive =
           "no-cache"                          ; Section 14.9.1
         | "no-store"                          ; Section 14.9.2
         | "max-age" "=" delta-seconds         ; Section 14.9.3, 14.9.4
         | "max-stale" [ "=" delta-seconds ]   ; Section 14.9.3
         | "min-fresh" "=" delta-seconds       ; Section 14.9.3
         | "no-transform"                      ; Section 14.9.5
         | "only-if-cached"                    ; Section 14.9.4
         | cache-extension                     ; Section 14.9.6    
            */

            var cacheControl = actionContext.Request.Headers.CacheControl;
            if (ShouldIgnoreCache(cacheControl, actionContext.Request))
            {
                return;
            }
            
            var invocation = GetInvocation(actionContext);
            var context = GetInvocationContext(actionContext);
            var scope = actionContext.Request.GetDependencyScope();
            var strategy = GetCacheStrategy(scope, invocation, context);
            
            var cacheKey = strategy.CacheKeyProvider.GetCacheKey(invocation, context);
            var hashedKey = HashCacheKey(cacheKey);
            var cacheStore = strategy.GetAsyncCacheStore(invocation, context);
            var storedKey = $"fw-{cacheStore.StoreId}-{hashedKey}";
            var builder = GetCacheResponseBuilder(scope);

            actionContext.Request.Properties[Global.__flatwhite_outputcache_store] = cacheStore;
            actionContext.Request.Properties[Global.__flatwhite_outputcache_key] = storedKey;
            actionContext.Request.Properties[Global.__flatwhite_outputcache_strategy] = strategy;
            actionContext.Request.Properties[WebApiExtensions.__webApi_outputcache_response_builder] = builder;

            var cacheItem = await cacheStore.GetAsync(storedKey).ConfigureAwait(false) as CacheItem;
            var response = builder.GetResponse(cacheControl, cacheItem, actionContext.Request);

            actionContext.Response = response;
        }

        /// <summary>
        /// Determine whether or not should ignore all the cache settings base on the 
        /// </summary>
        /// <param name="cacheControl"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual bool ShouldIgnoreCache(CacheControlHeaderValue cacheControl, HttpRequestMessage request)
        {
            return MaxAge == 0 || cacheControl != null && (
                                  cacheControl.NoCache ||
                                  cacheControl.NoStore ||
                                  cacheControl.MaxAge?.TotalSeconds == 0) && !IgnoreRevalidationRequest 
                               || 
                request.Properties.Any(prop => prop.Key.StartsWith(WebApiExtensions.__flatwhite_dont_cache_));
        }

        /// <summary>
        /// Store the response to cache store, add CacheControl and Etag to response
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (ShouldIgnoreCache(actionExecutedContext.Request.Headers.CacheControl, actionExecutedContext.Request))
            {
                return;
            }

            if ((actionExecutedContext.ActionContext.Response == null || !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode) && StaleIfError == 0)
            {
                return; // Early return
            }
            
            ApplyCacheHeaders(actionExecutedContext.ActionContext.Response, actionExecutedContext.Request);

            var storedKey = (string)actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_key];
            var cacheStore = (IAsyncCacheStore)actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_store];

            if (actionExecutedContext.ActionContext.Response == null || !actionExecutedContext.ActionContext.Response.IsSuccessStatusCode)
            {
                var cacheItem = await cacheStore.GetAsync(storedKey).ConfigureAwait(false) as CacheItem;
                if (cacheItem != null && StaleIfError > 0)
                {
                    var builder = (ICacheResponseBuilder)actionExecutedContext.Request.Properties[WebApiExtensions.__webApi_outputcache_response_builder];
                    var response = builder.GetResponse(actionExecutedContext.Request.Headers.CacheControl, cacheItem, actionExecutedContext.Request);

                    if (response != null)
                    {
                        //NOTE: Override error response
                        actionExecutedContext.Response = response;
                    }
                    return;
                }
            }

            var responseContent = actionExecutedContext.Response.Content;

            if (responseContent != null)
            {
                var cacheItem = new CacheItem(this)
                {
                    Key = storedKey,
                    Content = await responseContent.ReadAsByteArrayAsync().ConfigureAwait(false),
                    ResponseMediaType = responseContent.Headers.ContentType.MediaType,
                    ResponseCharSet = responseContent.Headers.ContentType.CharSet,
                    StoreId = cacheStore.StoreId
                };
                
                var strategy = (ICacheStrategy)actionExecutedContext.Request.Properties[Global.__flatwhite_outputcache_strategy];
                var invocation = GetInvocation(actionExecutedContext.ActionContext);
                var context = GetInvocationContext(actionExecutedContext.ActionContext);
                var changeMonitors = strategy.GetChangeMonitors(invocation, context);

                var objectContent = responseContent as ObjectContent;
                var phoenix = CreatePhoenix(invocation, cacheStore.StoreId, storedKey, objectContent?.Formatter);
                
                foreach (var mon in changeMonitors)
                {
                    mon.CacheMonitorChanged += x =>
                    {
                        phoenix.RebornOrDieForever(this);
                    };
                }

                actionExecutedContext.Response.Headers.ETag = new EntityTagHeaderValue($"\"{cacheItem.Key}-{cacheItem.Checksum}\"");

                var absoluteExpiration =  DateTime.UtcNow.AddSeconds(MaxAge + Math.Max(StaleWhileRevalidate, StaleIfError));
                await cacheStore.SetAsync(cacheItem.Key, cacheItem, absoluteExpiration).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Apply the CacheControl header to response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="request"></param>
        protected virtual void ApplyCacheHeaders(HttpResponseMessage response, HttpRequestMessage request)
        {
            /*
            cache-response-directive =
           "public"                               ; Section 14.9.1
         | "private" [ "=" <"> 1#field-name <"> ] ; Section 14.9.1
         | "no-cache" [ "=" <"> 1#field-name <"> ]; Section 14.9.1
         | "no-store"                             ; Section 14.9.2
         | "no-transform"                         ; Section 14.9.5
         | "must-revalidate"                      ; Section 14.9.4
         | "proxy-revalidate"                     ; Section 14.9.4
         | "max-age" "=" delta-seconds            ; Section 14.9.3
         | "s-maxage" "=" delta-seconds           ; Section 14.9.3
         | cache-extension                        ; Section 14.9.6
            */
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = MaxAge > 0 ? TimeSpan.FromSeconds(MaxAge) : (TimeSpan?) null,
                SharedMaxAge = SMaxAge > 0 ? TimeSpan.FromSeconds(SMaxAge) : (TimeSpan?)null,
                MustRevalidate = MustRevalidate,
                ProxyRevalidate = ProxyRevalidate,
                Private = Private,
                Public = Public,
                NoStore = NoStore,
                NoCache = NoCache,
                NoTransform = NoTransform,
            };

            if (NoCache)
            {
                response.Headers.Add("Pragma", "no-cache");
            }
        }

        /// <summary>
        /// Make a hash string of the original senstivive cacheKey
        /// </summary>
        /// <param name="originalCacheKey"></param>
        /// <returns></returns>
        protected virtual string HashCacheKey(string originalCacheKey)
        {
            return GetHashString(Encoding.ASCII.GetBytes(originalCacheKey));
        }

        private string GetHashString(byte[] content)
        {
            using (var md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(content).ToHex();
            }
        }

        /// <summary>
        /// Get <see cref="_IInvocation" /> from <see cref="HttpActionContext" />
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual _IInvocation GetInvocation(HttpActionContext actionContext)
        {
            return new WebApiInvocation(actionContext);
        }

        /// <summary>
        /// Get context data from <see cref="HttpActionContext" />
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> GetInvocationContext(HttpActionContext actionContext)
        {
            var provider = new WebApiContextProvider(actionContext);
            var context = provider.GetContext();
            context[Global.__flatwhite_outputcache_attribute] = this;
            context[WebApiExtensions.__webApi_dependency_scope] = actionContext.Request.GetDependencyScope();
            return context;
        }

        /// <summary>
        /// Create the phoenix object which can refresh the cache itself if StaleWhileRevalidate > 0
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheStoreId"></param>
        /// <param name="key"></param>
        /// <param name="mediaTypeFormatter">The formater that was used to create the reasponse at the first invocation</param>
        /// <returns></returns>
        protected virtual Phoenix CreatePhoenix(_IInvocation invocation, int cacheStoreId, string key, MediaTypeFormatter mediaTypeFormatter)
        {
            return new WebApiPhoenix(invocation, cacheStoreId, key, (int)MaxAge * 1000, (int)StaleWhileRevalidate * 1000, this);
        }
    }
}
