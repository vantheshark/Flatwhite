using System;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using NUnit.Framework;

namespace Flatwhite.Tests
{
    [TestFixture]
    public class CacheInterceptorTests
    {
        [SetUp]
        public void ShowSomeTrace()
        {
            Global.Cache = new MethodInfoCache();
        }
        [Test]
        public async Task Test_intercept_async_method()
        {
            var id1 = Guid.Parse("3dd19adf-b743-43d9-add4-19f85dc857da");

            var builder = new ContainerBuilder().EnableFlatwhiteCache();

            builder
                .RegisterType<BlogService>()
                .As<IBlogService>()
                .CacheWithAttribute();
            Global.CacheStoreProvider.RegisterStore(new UnitTestCacheStore());

            var container = builder.Build();

            var cachedService = container.Resolve<IBlogService>();

            for (var i = 0; i < 10; i++)
            {
                var name1 = await cachedService.GetByIdAsync(id1);
            }

            dynamic dService = cachedService;
            Assert.AreEqual(1, ((BlogService)dService.__target).InvokeCount);
        }
    }
}
