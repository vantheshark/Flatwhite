using Flatwhite.WebApi;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http.Controllers;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [DebuggerStepThrough]
    public class OutputCacheAttributeWithPublicMethods : Flatwhite.WebApi.OutputCacheAttribute
    {
        public ICacheStrategy GetCacheStrategyPublic(HttpRequestMessage request, _IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return this.GetCacheStrategy(request, invocation, invocationContext);
        }

        public bool ShouldIgnoreCachePublic(CacheControlHeaderValue cacheControl, HttpRequestMessage request)
        {
            return ShouldIgnoreCache(cacheControl, request);
        }

        public void ApplyCacheHeadersPublic(HttpResponseMessage response, HttpRequestMessage request)
        {
            ApplyResponseCacheHeaders(response, request);
        }

        public string HashCacheKeyPublic(string originalCacheKey)
        {
            return HashCacheKey(originalCacheKey);
        }

        public virtual _IInvocation GetInvocationPublic(HttpActionContext actionContext)
        {
            return GetInvocation(actionContext);
        }

        public IDictionary<string, object> GetInvocationContextPublic(HttpActionContext actionContext)
        {
            return GetInvocationContext(actionContext);
        }

        public void DisposeOldPhoenixAndCreateNew_Public(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage request)
        {
            var methodInfo = typeof(Flatwhite.WebApi.OutputCacheAttribute).GetMethod("DisposeOldPhoenixAndCreateNew", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this, new object[] { invocation, cacheItem, request });
        }
    }
}
