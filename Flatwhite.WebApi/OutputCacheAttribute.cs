using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;

namespace Flatwhite.WebApi
{
    public class OutputCacheAttribute : ActionFilterAttribute
    {
        public Type CacheStrategyType { get; set; }
        
        public Type CacheProviderType { get; set; }

        /// <summary>
        /// Gets or sets the cache duration, in miliseconds.
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// A semicolon-separated list of strings that correspond to to parameter values
        /// </summary>
        public string VaryByParam { get; set; }

        /// <summary>
        /// Gets or sets the vary-by-custom value.
        /// </summary>
        public string VaryByCustom { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contextProvider = new WebApiContextProvider(actionContext);
            var invocation = new WebApiInvocation(actionContext);
            var context = contextProvider.GetContext();
            context[typeof(OutputCacheAttribute).Name] = this;

            var scope = actionContext.Request.GetDependencyScope();

            var strategy = GetCacheStrategy(scope, invocation, context);
            if (!strategy.CanIntercept(invocation, context) || strategy.GetCacheTime(invocation, context) <=0)
            {
                return;
            }

            var cachekey = strategy.CacheKeyProvider.GetCacheKey(invocation, context);
            var cacheProvider = GetCacheProvider(scope);
            if (!cacheProvider.Contains(cachekey)) return;

            // Apply response header
            var val = (byte[]) cacheProvider.Get(cachekey);
            if (val == null) return;

            actionContext.Response = actionContext.Request.CreateResponse();
            actionContext.Response.Content = new ByteArrayContent(val);

            // TODO: actionContext.Response.Content.Headers.ContentType = contenttype;
            // Apply Etag
            // ApplyCacheHeaders
        }

        private ICacheProvider GetCacheProvider(IDependencyScope scope)
        {
            return (CacheProviderType != null ? scope.GetService(CacheProviderType) as ICacheProvider : null) ?? Global.CacheProvider;
        }

        private ICacheStrategy GetCacheStrategy(IDependencyScope scope, _IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var strategy = CacheStrategyType != null ? scope.GetService(CacheStrategyType) as ICacheStrategy : null;
            if (strategy == null)
            {
                var strategyProvider = scope.GetService(typeof (ICacheStrategyProvider)) as ICacheStrategyProvider ??
                                       Global.CacheStrategyProvider;
                strategy = strategyProvider.GetStrategy(invocation, invocationContext);
            }
            if (strategy == null) throw new Exception("Cannot find caching strategy for this requiest");
            return strategy;
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}
