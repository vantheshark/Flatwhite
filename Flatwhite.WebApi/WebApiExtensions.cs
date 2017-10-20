using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Flatwhite.Provider;
using Flatwhite.WebApi.CacheControl;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Provide extension methods to enable Flatwhite WebApi cache
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class WebApiExtensions
    {
        // ReSharper disable InconsistentNaming
        internal static readonly string __webApi = "__webApi";

        internal static readonly string __webApi_etag_matched = "__flatwhite_webApi_etag_matched";
        internal static readonly string __webApi_cache_is_stale = "__flatwhite_webApi_cache_is_stale";
        internal static readonly string __cacheControl_flatwhite_force_refresh = "flatwhite-force-refresh";

        internal static readonly string __flatwhite_dont_cache = "__flatwhite_dont_cache";

        internal static FlatwhiteWebApiConfiguration _fwConfig = new FlatwhiteWebApiConfiguration();
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Create required components to use Flatwhite cache for WebApi
        /// </summary>
        /// <param name="config"></param>
        /// <param name="flatwhiteConfig"></param>
        public static HttpConfiguration UseFlatwhiteCache(this HttpConfiguration config,
            FlatwhiteWebApiConfiguration flatwhiteConfig = null)
        {
            _fwConfig = flatwhiteConfig ?? new FlatwhiteWebApiConfiguration();
            Global.CacheStrategyProvider = config.DependencyResolver.GetService(typeof(ICacheStrategyProvider)) as WebApiCacheStrategyProvider ?? new WebApiCacheStrategyProvider();
            Global.BackgroundTaskManager = config.DependencyResolver.GetService(typeof(IBackgroundTaskManager)) as IBackgroundTaskManager ?? new RegisteredTasks();

            var allHandlers = config.DependencyResolver.GetServices(typeof(ICachControlHeaderHandler)).OfType<ICachControlHeaderHandler>().ToList();

            if (allHandlers.All(h => !(h is EtagHeaderHandler)))
            {
                var cacheResponseBuilder =config.DependencyResolver.GetService(typeof(ICacheResponseBuilder)) as ICacheResponseBuilder ?? _fwConfig.ResponseBuilder;
                var etagHeaderHandler = new EtagHeaderHandler(cacheResponseBuilder);
                allHandlers.Add(etagHeaderHandler);
            }

            if (_fwConfig.IgnoreVaryCustomKeys && allHandlers.All(h => !(h is EvaluateServerCacheHandler)))
            {
                var cacheResponseBuilder = config.DependencyResolver.GetService(typeof(ICacheResponseBuilder)) as ICacheResponseBuilder ?? _fwConfig.ResponseBuilder;
                var serverCacheHandler = new EvaluateServerCacheHandler(cacheResponseBuilder);
                allHandlers.Add(serverCacheHandler);
            }

            var handlerProvider = config.DependencyResolver.GetService(typeof(ICachControlHeaderHandlerProvider)) as ICachControlHeaderHandlerProvider ?? new CachControlHeaderHandlerProvider();
            allHandlers.ForEach(h => handlerProvider.Register(h));

            config.MessageHandlers.Add(new CacheMessageHandler(handlerProvider));
            if (_fwConfig.EnableStatusController)
            {
                config.Routes.MapHttpRoute(
                    name: "_FlatwhiteStatus",
                    routeTemplate: "_flatwhite/{action}/{id}",
                    defaults: new {id = RouteParameter.Optional, controller = "FlatwhiteStatus"}
                );
            }

            config.Properties["_fwConfig"] = _fwConfig;
            return config;
        }

        /// <summary>
        /// Get current Flatwhite cache configuration
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static FlatwhiteWebApiConfiguration GetFlatwhiteCacheConfiguration(this HttpConfiguration config)
        {
            return config.Properties.ContainsKey(_fwConfig) 
                ? config.Properties["_fwConfig"] as FlatwhiteWebApiConfiguration 
                : _fwConfig;
        }

        internal static string GetUrlBaseCacheKey(this HttpRequestMessage request)
        {
            return $"{request.Method}:{request.RequestUri.PathAndQuery}:{request.Headers.Accept}";
        }

        /// <summary>
        /// Get <see cref="ICacheStrategy" /> from <see cref="IDependencyScope" /> if <see cref="IHaveCacheStrategyType.CacheStrategyType"/> has value
        /// <para>Otherwise resolve from <see cref="Global.CacheStrategyProvider"/></para>
        /// </summary>
        /// <param name="source">The attribute object</param>
        /// <param name="request"></param>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        internal static ICacheStrategy GetCacheStrategy(this IHaveCacheStrategyType source, HttpRequestMessage request, _IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            ICacheStrategy strategy = null;
            if (source.CacheStrategyType != null)
            {
                strategy = request.GetDependencyScope().GetService(source.CacheStrategyType) as ICacheStrategy;
                if (strategy?.GetType() != source.CacheStrategyType)
                {
                    throw new Exception($"Cannot find cache strategy type {source.CacheStrategyType.Name} from dependecy scope of this request {request.RequestUri.PathAndQuery}");
                }
            }

            if (strategy == null)
            {
                strategy = Global.CacheStrategyProvider.GetStrategy(invocation, invocationContext);
            }

            if (strategy == null) throw new Exception($"Cannot find cache strategy from Global.CacheStrategyProvider of this request {request.RequestUri.PathAndQuery}");
            return strategy;
        }
    }
}
