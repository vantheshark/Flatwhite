using System;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// Scan types that were decorated or have members decorated with <see cref="OutputCacheAttribute"/> and enable default cache Interceptor (<see cref="CacheInterceptor"/>) on them
    /// </summary>
    public class FlatwhiteBuilderInterceptModule : Module
    {
        /// <summary>
        /// Check all <see cref="IComponentRegistration"/> and enable Interface or Class interceptor on the component if the register services have <see cref="OutputCacheAttribute"/> 
        /// decorated on the type or methods
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new FlatwhiteCoreModule());
            //var rb = RegistrationBuilder.ForType<IInterceptor>();

            //var k = new RegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
            //    new TypedService(implementationType),
            //    new ConcreteReflectionActivatorData(implementationType),
            //    new SingleRegistrationStyle());

            //builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            builder.RegisterCallback(x =>
            {
                foreach (var r in x.Registrations)
                {
                    //var type = r.Activator.LimitType;
                    var types = r
                        .Services
                        .OfType<IServiceWithType>()
                        .Select(s => s.ServiceType).ToList();

                    var typesWithAttributes =
                           types.Where(type => type.GetCustomAttributes(typeof(OutputCacheAttribute), true).Any() ||
                                               type.GetMethods().SelectMany(p => p.GetCustomAttributes(typeof(OutputCacheAttribute), true)).Any()).ToList();

                    if (typesWithAttributes.Count == 0) continue;

                    /*
                    builder.RegistrationStyle.Id,               = registration.Id
                    builder.RegistrationData,
                        data.Lifetime,                          = registration.Lifetime
                        data.Sharing,                           = registration.Sharing
                        data.Ownership,                         = registration.Ownership
                        services,                               = registration.Services
                        data.Metadata                           = registration.Metadata
                    builder.ActivatorData.Activator,            = registration.Activator
                    builder.RegistrationData.Services,          = registration.Services
                    builder.RegistrationStyle.Target            = registration.Target
                    */

                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> rb;
                    //https://github.com/autofac/Autofac/blob/master/src/Autofac/Builder/RegistrationBuilder.cs#214
                    if (typesWithAttributes.Any(t => t.IsClass))
                    {
                        rb = RegistrationBuilder.ForType(typesWithAttributes.First(type => type.IsClass));
                        rb.EnableClassInterceptors().InterceptedBy(typeof (CacheInterceptorAdaptor));
                        ((ComponentRegistration)r).Activator = rb.ActivatorData.Activator;
                        // NOTE: Find where to set it to rb.ActivatorData.ImplementationType;
                    }
                    else
                    {
                        rb = RegistrationBuilder.ForType(typesWithAttributes.First(type => type.IsInterface));
                        rb.EnableInterfaceInterceptors().InterceptedBy(typeof(CacheInterceptorAdaptor));
                    }

                    foreach (var pair in rb.RegistrationData.Metadata)
                        r.Metadata[pair.Key] = pair.Value;
                    foreach (var p in rb.RegistrationData.PreparingHandlers)
                        r.Preparing += p;
                    foreach (var ac in rb.RegistrationData.ActivatingHandlers)
                        r.Activating += ac;
                    foreach (var ad in rb.RegistrationData.ActivatedHandlers)
                        r.Activated += ad;
                }
            });
        }
    }
}
