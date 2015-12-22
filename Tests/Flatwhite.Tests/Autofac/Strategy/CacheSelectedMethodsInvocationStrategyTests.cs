using System;
using System.Threading;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Provider;
using Flatwhite.Strategy;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac.Strategy
{
    [TestFixture]
    public class CacheSelectedMethodsInvocationStrategyTests
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
        }

        [Test]
        public void Test_cache_on_selected_method()
        {
            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterType<BlogService>()
                .As<IBlogService>()
                .CacheWithStrategy(
                    CacheStrategies.ForService<IBlogService>()
                        .ForMember(x => x.GetById(Argument.Any<Guid>()))
                        .Duration(1000)
                        .VaryByParam("postId")
                        
                        .ForMember(x => x.GetComments(Argument.Any<Guid>(), Argument.Any<int>()))
                        .Duration(2000)
                        .VaryByCustom("custom")
                        .VaryByParam("postId")
                );

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
        public void Test_cache_on_selected_method_with_change_monitor()
        {
            var blogService = Substitute.For<IBlogService>();
            var wait = new CountdownEvent(2);
            blogService.When(x => x.GetById(Arg.Any<Guid>())).Do(c =>
            {
                c.Returns(new object {});
                wait.Signal();
            });

            FlatwhiteCacheEntryChangeMonitor mon = null;
            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(blogService)
                .As<IBlogService>()
                .SingleInstance()
                .CacheWithStrategy(
                    CacheStrategies.ForService<IBlogService>()
                        .ForMember(x => x.GetById(Argument.Any<Guid>()))
                        .Duration(100000)
                        .StaleWhileRevalidate(500)
                        .VaryByParam("postId")
                        .WithCacheStore(0)
                        .WithRevalidationKey("posts")
                        .WithChangeMonitors((i, context) =>
                        {
                            mon = new FlatwhiteCacheEntryChangeMonitor("");
                            return new[] {mon};
                        })
                );

            var container = builder.Build();

            var cachedService = container.Resolve<IBlogService>();

            var id = Guid.NewGuid();
            for (var i = 0; i < 1000; i++)
            {
                var result = cachedService.GetById(id);
            }
            //NOTE: After this, the cache item should be refreshed by Phoenix
            mon.OnChanged(null);
            
            for (var i = 0; i < 1000; i++)
            {
                var result = cachedService.GetById(id);
            }
            //NOTE: Because the phoenix reborn is none-blocking (on a background thread), it may need time to let the Task finish.
            Assert.IsTrue(wait.Wait(2000));
            mon.Dispose();
        }

        [Test]
        public void Test_cache_on_selected_method_with_custom_cache_store_type()
        {
            var cacheStore = Substitute.For<ICacheStore>();
            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterType<BlogService>()
                .As<IBlogService>()
                .SingleInstance()
                .CacheWithStrategy(
                    CacheStrategies.ForService<IBlogService>()
                        .ForMember(x => x.GetById(Argument.Any<Guid>()))
                        .Duration(5)
                        .WithCacheStoreType(cacheStore.GetType())
                );

            builder.RegisterInstance(cacheStore).As<ICacheStore>();
            var container = builder.Build();

            var cachedService = container.Resolve<IBlogService>();
            var cacheStoreProvider = Substitute.For<ICacheStoreProvider>();
            cacheStoreProvider.GetCacheStore(Arg.Is<Type>(t => t == cacheStore.GetType())).Returns(cacheStore);
            Global.CacheStoreProvider = cacheStoreProvider;
            var id = Guid.NewGuid();
            for (var i = 0; i < 10; i++)
            {
                var result = cachedService.GetById(id);
            }

            cacheStore.Received().Get(Arg.Any<string>());
        }
    }
}
