using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Flatwhite.AutofacIntergration;
using Flatwhite.WebApi.Tests.Controllers;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Flatwhite.WebApi.Tests.Startup))]

namespace Flatwhite.WebApi.Tests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder().EnableFlatwhiteCache();
            builder.RegisterType<WebApiCacheStrategyProvider>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<ValuesController>().AsSelf().CacheWithAttribute();

            var container = builder.Build();

            app.UseAutofacMiddleware(container)
               .UseWebApi(CreateHttpConfiguration(container));

        }
        private static HttpConfiguration CreateHttpConfiguration(IContainer container)
        {
            var config = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            WebApiConfig.Register(config);

            return config;
        }
    }
}
