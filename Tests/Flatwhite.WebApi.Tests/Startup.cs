using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Flatwhite.AutofacIntergration;
using Flatwhite.Provider;
using Flatwhite.WebApi.CacheControl;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Flatwhite.WebApi.Tests.Startup))]

namespace Flatwhite.WebApi.Tests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            var container = BuildAutofacContainer(config);
            
            WebApiConfig.Register(config);

            app.UseWebApi(config)
               .UseFlatwhiteCache<Startup>(config)
               .UseAutofacMiddleware(container)
               ;

        }

        private IContainer BuildAutofacContainer(HttpConfiguration config)
        {
            var builder = new ContainerBuilder().EnableFlatwhiteCache();

            // This will also be set to Global.CacheStrategyProvider in UseFlatwhiteCache method
            builder.RegisterType<WebApiCacheStrategyProvider>().As<ICacheStrategyProvider>().SingleInstance();

            // This will also be set to Global.CacheAttributeProvider in UseFlatwhiteCache method
            builder.RegisterType<WebApiCacheAttributeProvider>().As<ICacheAttributeProvider>().SingleInstance();

            // This is required by EtagHeaderHandler
            builder.RegisterType<CacheResponseBuilder>().As<ICacheResponseBuilder>().SingleInstance();

            // This is required by CachControlHeaderHandlerProvider
            builder.RegisterType<EtagHeaderHandler>().As<ICachControlHeaderHandler>().SingleInstance();
            //NOTE: Register more instance of ICachControlHeaderHandler here

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            return container;
        }
    }
}
