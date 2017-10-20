using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Provider;
using Flatwhite.Strategy;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
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
        public async Task Test_cache_on_selected_method()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
            builder
                .RegisterType<BlogService>()
                .As<INoneCaheBlogService>()
                .CacheWithStrategy(
                    CacheStrategies.ForService<INoneCaheBlogService>()
                        .ForMember(x => x.GetByIdAsync(Argument.Any<Guid>()))
                        .Duration(1000)
                        .VaryByCustom("custom")
                        .VaryByParam("postId")
                );

            var container = builder.Build();

            var cachedService = container.Resolve<INoneCaheBlogService>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            for (var i = 0; i < 10; i++)
            {
                var b1 = await cachedService.GetByIdAsync(id1);
                var b2 = await cachedService.GetByIdAsync(id2);
            }

            dynamic blogSvc = cachedService;
            Assert.AreEqual(2, blogSvc.__target.InvokeCount);
        }

        [Test]
        public void Test_cache_on_selected_method_with_custom_cache_store_type()
        {
            var cacheStore = Substitute.For<ICacheStore>();
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FlatwhiteCoreModule());
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
