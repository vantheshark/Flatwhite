using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Castle.DynamicProxy;
using Flatwhite.Provider;
using Flatwhite.Strategy;
using Module = Autofac.Module;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// Core module
    /// </summary>
    public class FlatwhiteCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => Global.CacheKeyProvider);
            builder.Register(c => Global.CacheStoreProvider);
            builder.Register(c => Global.ContextProvider);
            builder.Register(c => Global.CacheStrategyProvider);
            builder.Register(c => Global.AttributeProvider);
            builder.Register(c => Global.HashCodeGeneratorProvider);
            builder.Register(c => Global.Logger);

            builder.RegisterType<DefaultCacheStrategy>().As<ICacheStrategy>();
            builder.RegisterType<MethodInterceptorAdaptor>()
                   .Keyed<IInterceptor>(typeof(MethodInterceptorAdaptor))
                   .Named<IInterceptor>(typeof(MethodInterceptorAdaptor).Name)
                   .AsSelf();

            builder.RegisterType<FlatwhiteStart>().AsImplementedInterfaces();
            builder.RegisterType<AutofacServiceActivator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterSource(new KeyInterceptorRegistrationSource());
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

    /// <summary>
    /// Dynamic resolve MethodInterceptorAdaptor as keyed IInterceptor
    /// </summary>
    internal class KeyInterceptorRegistrationSource : IRegistrationSource
    {
        public static readonly IDictionary<Guid, List<Tuple<MethodInfo, Attribute>>> DynamicAttributeCache = new ConcurrentDictionary<Guid, List<Tuple<MethodInfo, Attribute>>>();

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var keyedService = service as KeyedService;
            if (keyedService == null || (!typeof(IInterceptor).IsAssignableFrom(keyedService.ServiceType)))
            {
                // It's not a request for the IInterceptor
                return Enumerable.Empty<IComponentRegistration>();
            }

            Guid id;
            if (!Guid.TryParse(keyedService.ServiceKey.ToString(), out id))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var registration = new ComponentRegistration(
                id,
                new DelegateActivator(keyedService.ServiceType, (c, p) =>
                {
                    var attProvider = c.Resolve<IAttributeProvider>();
                    var contextProvider = c.Resolve<IContextProvider>();

                    if (!DynamicAttributeCache.ContainsKey(id))
                    {
                        Global.Logger.Info($"Couldn't find attributes cache for key {id}");
                        return new MethodInterceptorAdaptor(attProvider, contextProvider);
                    }

                    var dynamicAttributeProvider = new DynamicAttributeProvider(attProvider, () => DynamicAttributeCache[id]);
                    return new MethodInterceptorAdaptor(dynamicAttributeProvider, contextProvider);
                }),
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new[] {service},
                new Dictionary<string, object>());

            return new IComponentRegistration[] {registration};
        }

        public bool IsAdapterForIndividualComponents { get; } = false;
    }
}
