using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Flatwhite.AutofacIntergration;
using Flatwhite.Tests.Stubs;

// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac
{
    [TestFixture]
    public class FlatwhiteBuilderInterceptModuleTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
        }
        [Test]
        public void Test_service_registered_as_self_and_implemented_interfaces_should_have_class_interceptors_enabled()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<BlogService>()
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterModule<FlatwhiteBuilderInterceptModule>();
            
            var container = builder.Build();
           
            dynamic @interface = container.Resolve<IBlogService>();
            Assert.IsNotNull(@interface);
            var blog1 = @interface.GetById(Guid.Empty);
            
            var @class = container.Resolve<BlogService>();
            Assert.IsNotNull(@class);
            for (var i = 0; i < 10; i++)
            {
                var blog2 = @class.GetById(Guid.Empty);
            }
            // Because the cache was created for the first call to get blog1
            Assert.AreEqual(0, @class.InvokeCount);
        }

        [Test]
        public void Test_service_registered_as_self_only_should_have_class_interceptors_enabled()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<BlogService>()
                .AsSelf();

            builder.RegisterModule<FlatwhiteBuilderInterceptModule>();

            var container = builder.Build();

            var svc = container.Resolve<BlogService>();
            Assert.IsNotNull(svc);
            for (var i = 0; i < 10; i++)
            {
                var blog = svc.GetById(Guid.Empty);
            }
            Assert.AreEqual(1, svc.InvokeCount);
        }

        [Test]
        public void Test_service_registered_as_implement_interfaces_should_have_interface_interceptors_enabled_and_cacheRules_on_interface_applied()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterType<BlogService>()
                .AsImplementedInterfaces();

            builder.RegisterModule<FlatwhiteBuilderInterceptModule>();
            var container = builder.Build();

            var svc = container.Resolve<IBlogService>();
            Assert.IsNotNull(svc);
            const int callCount = 10;
            for (var i = 0; i < callCount; i++)
            {
                var data = svc.GetComments(Guid.Empty, 0).ToList();
            }

            dynamic dService = svc;
            Assert.AreEqual(callCount, ((BlogService)dService.__target).InvokeCount);
        }

        [Test]
        public void Test_service_registered_as_implement_interfaces_should_have_interface_interceptors_enabled()
        {

            var builder = new ContainerBuilder();
            builder
                .RegisterType<BlogService>()
                .EnableClassInterceptors()
                .AsImplementedInterfaces();

            builder.RegisterModule<FlatwhiteBuilderInterceptModule>();
            var container = builder.Build();

            var svc = container.Resolve<IBlogService>();
            Assert.IsNotNull(svc);
            for (var i = 0; i < 10; i++)
            {
                var blog = svc.GetById(Guid.Empty);
            }

            dynamic dService = svc;
            Assert.AreEqual(1, ((BlogService)dService.__target).InvokeCount);
        }
    }
}
