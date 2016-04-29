using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
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
        internal static readonly string __webApi_dependency_scope = "__webApi_dependency_scope";
        internal static readonly string __webApi_etag_matched = "__flatwhite_webApi_etag_matched";
        internal static readonly string __webApi_outputcache_response_builder = "__flatwhite_webApi_outputcache_response_builder";
        internal static readonly string __webApi_cache_is_stale = "__flatwhite_webApi_cache_is_stale";
        internal static readonly string __cacheControl_flatwhite_force_refresh = "flatwhite-force-refresh";

        
        /// <summary>
        /// __flatwhite_dont_cache_
        /// </summary>
        internal static readonly string __flatwhite_dont_cache = "__flatwhite_dont_cache";
        // ReSharper restore InconsistentNaming
        

        internal static FlatwhiteWebApiConfiguration _fwConfig = new FlatwhiteWebApiConfiguration();

        /// <summary>
        /// Create required components to use Flatwhite cache for WebApi
        /// </summary>
        /// <param name="config"></param>
        /// <param name="flatwhiteConfig"></param>
        public static HttpConfiguration UseFlatwhiteCache(this HttpConfiguration config, FlatwhiteWebApiConfiguration flatwhiteConfig = null)
        {
            _fwConfig = flatwhiteConfig ?? new FlatwhiteWebApiConfiguration();
            Global.CacheStrategyProvider = new WebApiCacheStrategyProvider();
            Global.BackgroundTaskManager = new RegisteredTasks();

            var allHandlers = config.DependencyResolver
                .GetServices(typeof(ICachControlHeaderHandler))
                .OfType<ICachControlHeaderHandler>()
                .ToList();

            if (allHandlers.All(h => h.GetType() != typeof(EtagHeaderHandler)))
            {
                var cacheResponseBuilder = config.DependencyResolver.GetService(typeof(ICacheResponseBuilder)) as ICacheResponseBuilder
                                         ?? new CacheResponseBuilder();
                var etagHeaderHandler = new EtagHeaderHandler(cacheResponseBuilder);
                allHandlers.Add(etagHeaderHandler);
            }

            var handlerProvider = config.DependencyResolver.GetService(typeof(ICachControlHeaderHandlerProvider)) as ICachControlHeaderHandlerProvider
                                ?? new CachControlHeaderHandlerProvider();
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
            return config;
        }
    }
}
