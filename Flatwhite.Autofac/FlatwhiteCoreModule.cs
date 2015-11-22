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
            builder.Register(c => Global.CacheStoreProvider);
            builder.Register(c => Global.ContextProvider);
            builder.Register(c => Global.CacheStrategyProvider);
            builder.Register(c => Global.AttributeProvider);
            builder.Register(c => Global.HashCodeGeneratorProvider);

            builder.RegisterType<DefaultCacheStrategy>().As<ICacheStrategy>();
            builder.RegisterType<MethodInterceptorAdaptor>()
                   .Keyed<IInterceptor>(typeof(MethodInterceptorAdaptor))
                   .Named<IInterceptor>(typeof(MethodInterceptorAdaptor).Name)
                   .AsSelf();

            builder.RegisterType<FlatwhiteStart>().AsImplementedInterfaces();
            builder.RegisterType<AutofacServiceActivator>().AsImplementedInterfaces().SingleInstance();
        }
    }

    internal class FlatwhiteStart : IStartable
    {
        private readonly IComponentContext _containerContext;

        public FlatwhiteStart(IComponentContext containerContext)
        {
            _containerContext = containerContext;
        }

        public void Start()
        {
            Global.ServiceActivator = new AutofacServiceActivator(_containerContext);
        }
    }
}
