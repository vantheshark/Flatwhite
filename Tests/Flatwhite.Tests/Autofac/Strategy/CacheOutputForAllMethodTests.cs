using System;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Strategy;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac.Strategy
{
    [TestFixture]
    public class CacheOutputForAllMethodTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());

        }
        [Test]
        public void Test_cache_on_method_decorated_with_output_cache_attribute()
        {
            var id1 = Guid.Parse("3dd19adf-b743-43d9-add4-19f85dc857da");
            var id2 = Guid.Parse("d5be1152-92b8-4a7f-a837-5365d7c73001");
            var svc = Substitute.For<IUserService>();
            svc.GetById(id1).Returns(new { Name = "Van", Email = "van@gmail.com", Id = id1 });
            svc.GetById(id2).Returns(new { Name = "Billy", Email = "billy@gmail.com", Id = id2 });

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());

            builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var cachedService = container.Resolve<IUserService>();

            for (var i = 0; i < 10; i++)
            {
                var name1 = cachedService.GetById(id1);
                var name2 = cachedService.GetById(id2);
            }

            svc.Received(1).GetById(Arg.Is(id1));
            svc.Received(1).GetById(Arg.Is(id2));
        }

        [Test]
        public void Test_cache_all_method_strategy_on_registered_intance_service()
        {
            var id1 = Guid.Parse("3dd19adf-b743-43d9-add4-19f85dc857da");
            var id2 = Guid.Parse("d5be1152-92b8-4a7f-a837-5365d7c73001");
            var svc = Substitute.For<IUserService>();
            svc.GetById(id1).Returns(new { Name = "Van", Email = "van@gmail.com", Id = id1 });
            svc.GetById(id2).Returns(new { Name = "Billy", Email = "billy@gmail.com", Id = id2 });

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .CacheWithStrategy(CacheStrategies.AllMethods().VaryByParam("userId").VaryByCustom("custom").Duration(10));

            var container = builder.Build();

            var cachedService = container.Resolve<IUserService>();

            for (var i = 0; i < 10; i++)
            {
                var name1 = cachedService.GetById(id1);
                var name2 = cachedService.GetById(id2);
            }

            svc.Received(1).GetById(Arg.Is(id1));
            svc.Received(1).GetById(Arg.Is(id2));
        }

        [Test]
        public void Test_cache_all_method_strategy_on_registered_interface_type_service()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterType<BlogService>()
                .As<IBlogService>()
                .CacheWithStrategy(CacheStrategies.AllMethods().VaryByParam("postId"));

            var container = builder.Build();

            var cachedService = container.Resolve<IBlogService>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            for (var i = 0; i < 10; i++)
            {
                var b1 = cachedService.GetById(id1);
                var b2 = cachedService.GetById(id2);
            }

            dynamic blogSvc = cachedService;
            Assert.AreEqual(2, blogSvc.__target.InvokeCount);
        }

        [Test]
        public void Test_cache_all_method_strategy_on_registered_class_type_service()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());

            builder
                .RegisterType<BlogService>()
                .CacheWithStrategy(CacheStrategies.AllMethods().VaryByParam("postId"));

            var container = builder.Build();

            var cachedService = container.Resolve<BlogService>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            for (var i = 0; i < 10; i++)
            {
                var b1 = cachedService.GetById(id1);
                var b2 = cachedService.GetById(id2);
            }
            Assert.AreEqual(2, cachedService.InvokeCount);
        }

        [Test]
        public void Test_cache_all_method_strategy_should_obay_nocache_attribute()
        {
            var svc = Substitute.For<IUserService>();
            string email = "van@gmail.com";
            svc.GetByEmail(email).Returns(new {Name = "Van", Email = email, Id = "1"});

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .CacheWithStrategy(CacheStrategies.AllMethods().VaryByParam("email"));
            var container = builder.Build();

            var cachedService = container.Resolve<IUserService>();

            for (var i = 0; i < 10; i++)
            {
                dynamic user = cachedService.GetByEmail(email);
                Assert.AreEqual("Van", user.Name);
            }

            svc.Received(10).GetByEmail(Arg.Is(email));
        }
    }
}
