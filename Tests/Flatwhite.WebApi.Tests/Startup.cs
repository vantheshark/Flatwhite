using System.Reflection;
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
            var builder = new ContainerBuilder().EnableFlatwhite();

            // This will also be set to Global.CacheStrategyProvider in UseFlatwhiteCache method
            builder.RegisterType<WebApiCacheStrategyProvider>().As<ICacheStrategyProvider>().SingleInstance();

            // This is required by EtagHeaderHandler and OutputCacheAttribute when it builds the response
            builder.RegisterType<CacheResponseBuilder>().As<ICacheResponseBuilder>().SingleInstance();

            // This is required by CachControlHeaderHandlerProvider
            // NOTE: Register more instances of ICachControlHeaderHandler here
            builder.RegisterType<EtagHeaderHandler>().As<ICachControlHeaderHandler>().SingleInstance();
            
            



            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            return container;
        }
    }
}
