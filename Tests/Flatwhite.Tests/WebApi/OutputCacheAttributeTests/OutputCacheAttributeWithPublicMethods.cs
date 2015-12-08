using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Flatwhite.WebApi;

namespace Flatwhite.Tests.WebApi.OutputCacheAttributeTests
{
    [DebuggerStepThrough]
    public class OutputCacheAttributeWithPublicMethods : Flatwhite.WebApi.OutputCacheAttribute
    {
        public ICacheStrategy GetCacheStrategyPublic(IDependencyScope scope, _IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            return GetCacheStrategy(scope, invocation, invocationContext);
        }

        public ICacheResponseBuilder GetCacheResponseBuilderPublic(IDependencyScope scope)
        {
            return GetCacheResponseBuilder(scope);
        }

        public bool ShouldIgnoreCachePublic(CacheControlHeaderValue cacheControl, HttpRequestMessage request)
        {
            return ShouldIgnoreCache(cacheControl, request);
        }

        public void ApplyCacheHeadersPublic(HttpResponseMessage response, HttpRequestMessage request)
        {
            ApplyCacheHeaders(response, request);
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

        public void CreatePhoenixPublic(_IInvocation invocation, WebApiCacheItem cacheItem, HttpRequestMessage request, MediaTypeFormatter mediaTypeFormatter)
        {
            var methodInfo = typeof(Flatwhite.WebApi.OutputCacheAttribute).GetMethod("CreatePhoenix", BindingFlags.Instance | BindingFlags.NonPublic);
            methodInfo.Invoke(this, new object[] { invocation, cacheItem, request, mediaTypeFormatter });
        }
    }
}
