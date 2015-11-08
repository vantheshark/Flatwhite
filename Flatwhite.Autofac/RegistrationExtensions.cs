using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;

namespace Flatwhite.AutofacIntergration
{
    /// https://github.com/autofac/Autofac.Extras.DynamicProxy/blob/master/src/Autofac.Extras.DynamicProxy/RegistrationExtensions.cs
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    public static class RegistrationExtensions
    {
        static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        const string InterceptorsPropertyName = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";

        static readonly IEnumerable<Service> EmptyServices = new Service[0];

        /// <summary>
        /// Enable class interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or added with InterceptedBy().
        /// Only virtual methods can be intercepted this way.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TConcreteReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>EnableClassInterceptors<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration) 
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {
            return EnableClassInterceptors(registration, ProxyGenerationOptions.Default);
        }

        /// <summary>
        /// Enable class interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or added with InterceptedBy().
        /// Only virtual methods can be intercepted this way.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <typeparam name="TConcreteReflectionActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <param name="options">Proxy generation options to apply.</param>
        /// <param name="additionalInterfaces">Additional interface types. Calls to their members will be proxied as well.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> EnableClassInterceptors<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration, ProxyGenerationOptions options, params Type[] additionalInterfaces)  
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }
            registration.ActivatorData.ImplementationType =
            ProxyGenerator.ProxyBuilder.CreateClassProxyType(
            registration.ActivatorData.ImplementationType, additionalInterfaces ?? new Type[0], options);

            registration.OnPreparing(e =>
            {
                var proxyParameters = new List<Parameter>();
                var index = 0;

                if (options.HasMixins)
                {
                    foreach (var mixin in options.MixinData.Mixins)
                    {
                        proxyParameters.Add(new PositionalParameter(index++, mixin));
                    }
                }

                proxyParameters.Add(new PositionalParameter(index++, GetInterceptorServices(e.Component, registration.ActivatorData.ImplementationType)
                .Select(s => e.Context.ResolveService(s))
                .Cast<IInterceptor>()
                .ToArray()));

                if (options.Selector != null)
                {
                    proxyParameters.Add(new PositionalParameter(index, options.Selector));
                }

                e.Parameters = proxyParameters.Concat(e.Parameters).ToArray();
            });

            return registration;
        }

        /// <summary>
        /// Enable interface interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or interface, or added with InterceptedBy() calls.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>EnableInterfaceInterceptors<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
        {
            return EnableInterfaceInterceptors(registration, null);
        }

        /// <summary>
        /// Enable interface interception on the target type. Interceptors will be determined
        /// via Intercept attributes on the class or interface, or added with InterceptedBy() calls.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to apply interception to.</param>
        /// <param name="options">Proxy generation options to apply.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle>EnableInterfaceInterceptors<TLimit, TActivatorData, TSingleRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration, ProxyGenerationOptions options)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }
            registration.RegistrationData.ActivatingHandlers.Add((sender, e) =>
            {
                EnsureInterfaceInterceptionApplies(e.Component);

                var proxiedInterfaces = e.Instance.GetType().GetInterfaces()
                                .Where(i => (i.IsVisible || i.Assembly.IsInternalToDynamicProxy()) && !i.Assembly.FullName.StartsWith("Castle.")).ToArray();

                if (!proxiedInterfaces.Any())
                    return;

                var theInterface = proxiedInterfaces.First();
                var interfaces = proxiedInterfaces.Skip(1).ToArray();

                var interceptors = GetInterceptorServices(e.Component, e.Instance.GetType())
                .Select(s => e.Context.ResolveService(s))
                .Cast<IInterceptor>()
                .ToArray();

                e.Instance = options == null
                ? ProxyGenerator.CreateInterfaceProxyWithTarget(theInterface, interfaces, e.Instance, interceptors)
                : ProxyGenerator.CreateInterfaceProxyWithTarget(theInterface, interfaces, e.Instance, options, interceptors);
            });

            return registration;
        }

        static void EnsureInterfaceInterceptionApplies(IComponentRegistration componentRegistration)
        {
            if (componentRegistration.Services
                .OfType<IServiceWithType>()
                .Any(swt => !swt.ServiceType.IsInterface || (!swt.ServiceType.Assembly.IsInternalToDynamicProxy() && !swt.ServiceType.IsVisible)))
                    throw new InvalidOperationException($"The component {componentRegistration} cannot use interface interception as it provides services that are not publicly visible interfaces. Check your registration of the component to ensure you're not enabling interception and registering it as an internal/private interface type.");
        }

        static IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration, Type implType)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (implType == null)
                throw new ArgumentNullException(nameof(implType));

            var result = EmptyServices;

            object services;
            if (registration.Metadata.TryGetValue(InterceptorsPropertyName, out services))
                result = result.Concat((IEnumerable<Service>)services);

            //if (implType.IsClass)
            //{
            //    result = result.Concat(implType
            //    .GetCustomAttributes(typeof(InterceptAttribute), true)
            //    .Cast<InterceptAttribute>()
            //    .Select(att => att.InterceptorService));

            //    result = result.Concat(implType.GetInterfaces()
            //    .SelectMany(i => i.GetCustomAttributes(typeof(InterceptAttribute), true))
            //    .Cast<InterceptAttribute>()
            //    .Select(att => att.InterceptorService));
            //}

            return result.ToArray();
        }

        /// <summary>
        /// Allows a list of interceptor services to be assigned to the registration.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="builder">Registration to apply interception to.</param>
        /// <param name="interceptorServices">The interceptor services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">builder or interceptorServices</exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>InterceptedBy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, params Service[] interceptorServices)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (interceptorServices == null || interceptorServices.Any(s => s == null))
                throw new ArgumentNullException(nameof(interceptorServices));

            object existing;
            if (builder.RegistrationData.Metadata.TryGetValue(InterceptorsPropertyName, out existing))
                builder.RegistrationData.Metadata[InterceptorsPropertyName] =
                ((IEnumerable<Service>)existing).Concat(interceptorServices).Distinct();
            else
                builder.RegistrationData.Metadata.Add(InterceptorsPropertyName, interceptorServices);

            return builder;
        }

        /// <summary>
        /// Allows a list of interceptor services to be assigned to the registration.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="builder">Registration to apply interception to.</param>
        /// <param name="interceptorServiceTypes">The types of the interceptor services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">builder or interceptorServices</exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>InterceptedBy<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, params Type[] interceptorServiceTypes)
        {
            if (interceptorServiceTypes == null || interceptorServiceTypes.Any(t => t == null))
                throw new ArgumentNullException(nameof(interceptorServiceTypes));

            return InterceptedBy(builder, interceptorServiceTypes.Select(t => new TypedService(t)).ToArray());
        }
    }
}
