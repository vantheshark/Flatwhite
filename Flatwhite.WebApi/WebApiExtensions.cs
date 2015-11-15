using System.Linq;
using System.Web.Http;
using Flatwhite.WebApi.CacheControl;
using Microsoft.Owin.Logging;
using Owin;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Provide extension methods to enable Flatwhite WebApi cache
    /// </summary>
    public static class WebApiExtensions
    {
        // ReSharper disable InconsistentNaming
        internal static readonly string __webApi = "__webApi";
        internal static readonly string __webApi_etag_matched = "__flatwhite_webApi_etag_matched";
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Create required components to use Flatwhite cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="app"></param>
        /// <param name="config"></param>
        public static IAppBuilder UseFlatwhiteCache<T>(this IAppBuilder app, HttpConfiguration config)
        {
            Global.CacheStrategyProvider = new WebApiCacheStrategyProvider();
            Global.CacheAttributeProvider = new WebApiCacheAttributeProvider();

            var allHandlers = config.DependencyResolver
                .GetServices(typeof (ICachControlHeaderHandler))
                .OfType<ICachControlHeaderHandler>()
                .ToList();

            if (allHandlers.All(h => h.GetType() != typeof (EtagHeaderHandler)))
            {
                var cacheResponseBuilder = config.DependencyResolver.GetService(typeof(ICacheResponseBuilder)) as ICacheResponseBuilder 
                                         ?? new CacheResponseBuilder();
                var etagHeaderHandler = new EtagHeaderHandler(cacheResponseBuilder);
                allHandlers.Add(etagHeaderHandler);
            }

            var handlerProvider = config.DependencyResolver.GetService(typeof(ICachControlHeaderHandlerProvider)) as ICachControlHeaderHandlerProvider 
                                ?? new CachControlHeaderHandlerProvider();
            allHandlers.ForEach(h => handlerProvider.Register(h));

            var logger = app.CreateLogger<T>();
            config.MessageHandlers.Add(new CacheMessageHandler(handlerProvider, logger));
            return app;
        }
    }
}
