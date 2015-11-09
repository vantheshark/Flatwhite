using Autofac;
using Castle.DynamicProxy;
using Flatwhite.Strategy;

namespace Flatwhite.AutofacIntergration
{
    internal class FlatwhiteCoreModule : Module
    { 
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => Global.CacheKeyProvider);
            builder.Register(c => Global.CacheProvider);
            builder.Register(c => Global.ContextProvider);
            builder.Register(c => Global.CacheStrategyProvider);
            builder.Register(c => Global.AttributeProvider);
            builder.Register(c => Global.CacheAttributeProvider);
            

            builder.RegisterType<DefaultCacheStrategy>().As<ICacheStrategy>();
            builder.RegisterType<CacheInterceptor>()
                   .Keyed<IInterceptor>(typeof(CacheInterceptor))
                   .AsSelf();
        }
    }
}
