using System;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Strategy;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Flatwhite.Tests.Autofac
{
    [TestFixture]
    public class KeyInterceptorRegistrationSourceTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());

        }
        [Test]
        public async Task Should_not_throw_exception_when_cannot_resolve_KeyService_by_id()
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
            KeyInterceptorRegistrationSource.DynamicAttributeCache.Clear();
            var cachedService = container.Resolve<INoneCaheBlogService>();


            var id = Guid.NewGuid();
            for (var i = 0; i < 10; i++)
            {
                var name1 = await cachedService.GetByIdAsync(id);
                
            }
            dynamic blogSvc = cachedService;
            Assert.AreEqual(10, blogSvc.__target.InvokeCount);
        }
    }
}
