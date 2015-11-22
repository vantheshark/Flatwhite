using System;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Strategy;
using Flatwhite.Tests.Stubs;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac.Strategy
{
    [TestFixture]
    public class CacheSelectedMethodsInvocationStrategyTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
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
            FlatwhiteCacheEntryChangeMonitor mon = null;
            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterType<BlogService>()
                .As<IBlogService>()
                .CacheWithStrategy(
                    CacheStrategies.ForService<IBlogService>()
                        .ForMember(x => x.GetById(Argument.Any<Guid>()))
                        .Duration(5000)
                        .VaryByParam("postId")
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

            dynamic blogSvc = cachedService;
            Assert.AreEqual(1, blogSvc.__target.InvokeCount);
            
            mon.OnChanged(null);
            //NOTE: After this, the cache item should be removed
            for (var i = 0; i < 1000; i++)
            {
                var result = cachedService.GetById(id);
            }
            Assert.AreEqual(2, blogSvc.__target.InvokeCount);
        }
    }
}
