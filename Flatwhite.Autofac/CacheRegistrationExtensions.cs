using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.DynamicProxy;
using Flatwhite.Strategy;

namespace Flatwhite.AutofacIntergration
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type to support Flatwhite output cache
    /// </summary>
    public static class CacheRegistrationExtensions
    {
        private static readonly string FlatwhiteFilterAttributes = "FlatwhiteFilterAttributes";

        /// <summary>
        /// Cache with a custom <see cref="ICacheStrategy"/>
        /// <para>NOTE: Call method <see cref="EnableFlatwhite"/> once on the <see cref="ContainerBuilder"/> after created to enable Flatwhite caching</para>
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="builder">Registration to apply interception to.</param>
        /// <param name="strategy"></param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">builder or interceptorServices</exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> CacheWithStrategy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, IDynamicCacheStrategy strategy)
        {
            var methodAttributes = strategy.GetCacheAttributes().Select(k => new Tuple<MethodInfo, Attribute>(k.Key, k.Value)).ToArray();
            return FilterWithAttributes(builder, methodAttributes);
        }


        /// <summary>
        /// Dynamically create a method interceptor that will use provided filter attributes during interception
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TStyle"></typeparam>
        /// <param name="builder"></param>
        /// <param name="filterAttributes"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> FilterWith<TLimit, TActivatorData, TStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, params Attribute[] filterAttributes)
        {
            return FilterWithAttributes(builder, filterAttributes.Select(a => new Tuple<MethodInfo, Attribute>(null, a)).ToArray());
        }

        /// <summary>
        /// Dynamically create a method interceptor that will use provided filter attributes during interception
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TStyle"></typeparam>
        /// <param name="builder"></param>
        /// <param name="filterAttributes"></param>
        /// <returns></returns>
        internal static IRegistrationBuilder<TLimit, TActivatorData, TStyle> FilterWithAttributes<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, Tuple<MethodInfo, Attribute>[] filterAttributes)
        {
            bool firstTime = false;
            if (!builder.RegistrationData.Metadata.ContainsKey(FlatwhiteFilterAttributes))
            {
                builder.RegistrationData.Metadata[FlatwhiteFilterAttributes] = new List<Tuple<MethodInfo, Attribute>>();
                firstTime = true;
            }
            var filters = (List<Tuple<MethodInfo, Attribute>>)builder.RegistrationData.Metadata[FlatwhiteFilterAttributes];
            filters.AddRange(filterAttributes);
            if (!firstTime)
            {
                return builder;
            }
            
            var id = Guid.NewGuid();
            KeyInterceptorRegistrationSource.DynamicAttributeCache[id] = filters;

            var types = builder
                .RegistrationData
                .Services
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
        /// This is a shortcut of calling following methods on a registration builder
        /// <para/>         .EnableClassInterceptors()
        ///                 .EnableInterfaceInterceptors()
        ///                 .InterceptedBy(typeof(MethodInterceptorAdaptor))
        /// 
        /// <para>NOTE: Call method <see cref="EnableFlatwhite"/> once on the <see cref="ContainerBuilder"/> after created to enable Flatwhite interceptors</para>
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TStyle"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> EnableInterceptors<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder)
        {
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
            return builder.InterceptedBy(typeof(MethodInterceptorAdaptor));
        }

        /// <summary>
        /// Call this method once after you create <see cref="ContainerBuilder"/> to enable Flatwhite method interceptors
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        [Obsolete("You can simply register FlatwhiteCoreModule()")]
        public static ContainerBuilder EnableFlatwhite(this ContainerBuilder builder)
        {
            builder.RegisterModule(new FlatwhiteCoreModule());
            return builder;
        }
    }
}
