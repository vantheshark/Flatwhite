using System;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type to support Flatwhite output cache
    /// </summary>
    public static class CacheRegistrationExtensions
    {
        private static ContainerBuilder _currentBuilder;
        /// <summary>
        /// Cache with a custom <see cref="ICacheStrategy"/>
        /// <para>NOTE: Call method <see cref="EnableFlatwhiteCache"/> once on the <see cref="ContainerBuilder"/> after created to enable Flatwhite caching</para>
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="builder">Registration to apply interception to.</param>
        /// <param name="strategy"></param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">builder or interceptorServices</exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>CacheWithStrategy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, ICacheStrategy strategy) 
        {
            if (_currentBuilder == null)
            {
                throw new InvalidOperationException($"Please call {nameof(EnableFlatwhiteCache)} on ContainerBuilder first!");
            }

            var interceptor = new CacheInterceptorAdaptor(Global.ContextProvider, strategy);
            var id = Guid.NewGuid();
            _currentBuilder.RegisterInstance(interceptor).Keyed<IInterceptor>(id);
            
            var types = builder.RegistrationData.Services
                                .OfType<IServiceWithType>()
                                .Select(s => s.ServiceType).ToList();
            if (types.Any(t => t.IsClass) && builder.ActivatorData is ConcreteReflectionActivatorData)
            {
                 ((IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TStyle>)builder).EnableClassInterceptors();
            }
            else
            {
                builder.EnableInterfaceInterceptors();
            }
            return builder.InterceptedBy(new KeyedService(id, typeof(IInterceptor)));
        }

        /// <summary>
        /// Cache rule will be based on the decorated <see cref="OutputCacheAttribute"/> on type or public members
        /// <para>NOTE: Call method <see cref="EnableFlatwhiteCache"/> once on the <see cref="ContainerBuilder"/> after created to enable Flatwhite caching</para>
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TStyle"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> CacheWithAttribute<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder)
        {
            if (_currentBuilder == null)
            {
                throw new InvalidOperationException($"Please call {nameof(EnableFlatwhiteCache)} on ContainerBuilder first!");
            }

            var types = builder.RegistrationData.Services
                                .OfType<IServiceWithType>()
                                .Select(s => s.ServiceType).ToList();
            if (types.Any(t => t.IsClass) && builder.ActivatorData is ConcreteReflectionActivatorData)
            {
                ((IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TStyle>)builder).EnableClassInterceptors();
            }
            else
            {
                builder.EnableInterfaceInterceptors();
            }
            return builder.InterceptedBy(typeof(CacheInterceptorAdaptor));
        }

        /// <summary>
        /// Call this method once after you create <see cref="ContainerBuilder"/> to enable Flatwhite caching
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder EnableFlatwhiteCache(this ContainerBuilder builder)
        {
            builder.RegisterModule(new FlatwhiteCoreModule());
            _currentBuilder = builder;
            return builder;
        }
    }
}
