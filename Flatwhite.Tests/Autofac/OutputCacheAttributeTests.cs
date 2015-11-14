using System;
using Autofac;
using NUnit.Framework;
using Flatwhite.AutofacIntergration;
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
        }
        [Test]
        public void Test_vary_by_param()
        {
            var id1 = Guid.Parse("3dd19adf-b743-43d9-add4-19f85dc857da");
            var id2 = Guid.Parse("d5be1152-92b8-4a7f-a837-5365d7c73001");
            var svc = Substitute.For<IUserService>();
            svc.GetById(id1).Returns(new { Name = "Van", Email = "van@gmail.com", Id = id1 });
            svc.GetById(id2).Returns(new { Name = "Billy", Email = "billy@gmail.com", Id = id2 });

            var builder = new ContainerBuilder().EnableFlatwhiteCache();
            builder
                .RegisterInstance(svc)
                .AsImplementedInterfaces()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(CacheInterceptorAdaptor));
            Global.CacheStoreProvider.RegisterStore(new UnitTestCacheStore());
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
    }
}
