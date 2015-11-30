using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Hot;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Hot
{
    [TestFixture]
    public class PhoenixTests
    {
        private readonly OutputCacheAttribute _cacheAttribute = new OutputCacheAttribute();
        private _IInvocation _invocation;
        private readonly int _storeId = 100;

        private static readonly CacheInfo CacheInfo = new CacheInfo
        {
            CacheKey = "cacheKey",
            CacheStoreId = 100,
            CacheDuration = 5000,
            StaleWhileRevalidate = 5000
        };

        public AutoResetEvent SetUp(string method)
        {
            var svc = Substitute.For<IUserService>();
            svc.GetById(Arg.Any<Guid>()).Returns(cx => cx.Arg<Guid>());
            svc.GetByIdAsync(Arg.Any<Guid>()).Returns(cx => Task.FromResult(cx.Arg<Guid>()));

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .EnableInterceptors();
            var container = builder.Build();
            var proxy = container.Resolve<IUserService>();

            _invocation = Substitute.For<_IInvocation>();
            _invocation.Arguments.Returns(new object[] { Guid.NewGuid() });
            _invocation.Method.Returns(typeof(IUserService).GetMethod(method, BindingFlags.Instance | BindingFlags.Public));
            _invocation.Proxy.Returns(proxy);

            Global.Cache = new MethodInfoCache();
            var cacheStore = Substitute.For<ICacheStore>();
            var autoResetEvent = new AutoResetEvent(false);
            cacheStore.When(x => x.Set("cacheKey", Arg.Is<object>(obj => obj != null), Arg.Any<DateTimeOffset>()))
                .Do(c => autoResetEvent.Set());
            cacheStore.StoreId.Returns(_storeId);
            Global.CacheStoreProvider.RegisterStore(cacheStore);

            return autoResetEvent;
        }

        [Test]
        public void The_method_Reborn_should_try_to_refresh_the_cache()
        {
            // Arrange
            var wait = SetUp("GetById");
            var phoenix = new Phoenix(_invocation, CacheInfo);

            // Action
            phoenix.Reborn();

            // Assert
            Assert.IsTrue(wait.WaitOne(2000));
            Global.CacheStoreProvider.GetCacheStore(_storeId).Received(1)
                .Set("cacheKey", Arg.Is<object>(obj => obj != null), Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void The_method_Reborn_should_work_with_async_method()
        {
            // Arrange
            var wait = SetUp("GetByIdAsync");
            var phoenix = new Phoenix(_invocation, CacheInfo);

            // Action
            
            phoenix.Reborn();

            // Assert
            Assert.IsTrue(wait.WaitOne(2000));
            Global.CacheStoreProvider.GetCacheStore(_storeId).Received(1)
                .Set("cacheKey", Arg.Is<object>(obj => obj != null), Arg.Any<DateTimeOffset>());
        }
    }
}
