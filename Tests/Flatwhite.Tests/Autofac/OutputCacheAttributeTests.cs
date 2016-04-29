using System;
using System.Threading;
using Autofac;
using NUnit.Framework;
using Flatwhite.AutofacIntergration;
using Flatwhite.Tests.Stubs;
using NSubstitute;
// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac
{
    [TestFixture]
    public class OutputCacheAttributeTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
        }

        [Test]
        public void Test_vary_by_param()
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
                .AsImplementedInterfaces()
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
        public void Test_vary_custom_store_id()
        {
            var svc = Substitute.For<IUserService>();
            svc.TestCustomStoreId().Returns(new { Name = "Van", Email = "van@gmail.com" });

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterInstance(svc)
                .AsImplementedInterfaces()
                .EnableInterceptors();

            var container = builder.Build();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore(100));
            var cachedService = container.Resolve<IUserService>();


            for (var i = 0; i < 10; i++)
            {
                var obj = cachedService.TestCustomStoreId();
                Assert.IsNotNull(obj);
            }

            svc.Received(1).TestCustomStoreId();
        }

        [Test]
        public void Test_revalidaton_key()
        {
            var count = new CountdownEvent(2);
            var id1 = Guid.Parse("3dd19adf-b743-43d9-add4-19f85dc857da");
            var svc = Substitute.For<IUserService>();
            svc.GetById(id1).Returns(new { Name = "Van", Email = "van@gmail.com", Id = id1 });
            svc.When(x => x.GetById(id1))
                .Do(c => { count.Signal(); });

            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterInstance(svc)
                .AsImplementedInterfaces()
                .EnableInterceptors();

            var container = builder.Build();
            var cachedService = container.Resolve<IUserService>();


            for (var i = 0; i < 10; i++)
            {
                var name1 = cachedService.GetById(id1);
            }
            svc.Received(1).GetById(Arg.Is(id1));

            // NOTE: After call this, the cache shoud be reset
            cachedService.DisableUser(id1);
            for (var i = 0; i < 10; i++)
            {
                var name1 = cachedService.GetById(id1);
            }

            Assert.IsTrue(count.Wait(2000));
            svc.Received(2).GetById(Arg.Is(id1));
        }
    }
}
