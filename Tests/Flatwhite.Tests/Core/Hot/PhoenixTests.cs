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
        private _IInvocation _invocation;
        private const int StoreId = 99999;
        private readonly Guid _id = Guid.NewGuid();
        private static readonly CacheItem CacheInfo = new CacheItem
        {
            Key = "cacheKey",
            StoreId = StoreId,
            MaxAge = 5000,
            StaleWhileRevalidate = 5000
        };

        public AutoResetEvent SetUp(string method, IUserService svc = null)
        {
            Global.Init();
            if (svc == null)
            {
                svc = Substitute.For<IUserService>();
                if (method == nameof(IUserService.GetById))
                {
                    svc.GetById(Arg.Any<Guid>()).Returns(cx => cx.Arg<Guid>());
                }

                if (method == nameof(IUserService.GetByIdAsync))
                {
                    svc.GetByIdAsync(Arg.Any<Guid>()).Returns(cx => Task.FromResult((object)cx.Arg<Guid>()));
                }
            }

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(svc)
                .As<IUserService>()
                .EnableInterceptors();
            var container = builder.Build();
            var proxy = container.Resolve<IUserService>();

            _invocation = Substitute.For<_IInvocation>();
            _invocation.Arguments.Returns(new object[] { _id });
            _invocation.Method.Returns(typeof(IUserService).GetMethod(method, BindingFlags.Instance | BindingFlags.Public));
            _invocation.Proxy.Returns(proxy);

            
            var cacheStore = Substitute.For<IAsyncCacheStore>();
            var autoResetEvent = new AutoResetEvent(false);
            cacheStore.When(x => x.SetAsync(CacheInfo.Key, Arg.Is<object>(obj => _id.Equals(((CacheItem)obj).Data)), Arg.Any<DateTimeOffset>()))
                .Do(c => autoResetEvent.Set());

            cacheStore.When(x => x.RemoveAsync(CacheInfo.Key))
                .Do(c => autoResetEvent.Set());


            cacheStore.StoreId.Returns(StoreId);
            Global.CacheStoreProvider.RegisterAsyncStore(cacheStore);

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
            Global.CacheStoreProvider.GetAsyncCacheStore(StoreId).Received(1)
                .SetAsync("cacheKey", Arg.Is<object>(obj => _id.Equals(((CacheItem) obj).Data)), Arg.Any<DateTimeOffset>());
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
            Global.CacheStoreProvider.GetAsyncCacheStore(StoreId).Received(1)
                .SetAsync("cacheKey", Arg.Is<object>(obj => _id.Equals(((CacheItem)obj).Data)), Arg.Any<DateTimeOffset>());
        }

        [Test]
        public void The_method_Reborn_should_dispose_and_return_DisposingPhoenix_if_cannot_create_new_cacheItem()
        {
            // Arrange
            var svc = Substitute.For<IUserService>();
            svc.GetById(Arg.Any<Guid>()).Returns(null);
            var wait = SetUp("GetById", svc);
            var phoenix = new Phoenix(_invocation, CacheInfo);

            // Action
            phoenix.Reborn();
            Assert.IsTrue(wait.WaitOne(2000));
            // Assert
            var storeId = Global.CacheStoreProvider.GetAsyncCacheStore(StoreId);
            storeId.Received(1).RemoveAsync("cacheKey");
        }

        [Test]
        public void The_method_Reborn_should_throw_if_error_and_retry_after_1_second()
        {
            // Arrange
            var svc = Substitute.For<IUserService>();
            var wait = SetUp("GetById", svc);
            svc.When(x => x.GetById(Arg.Any<Guid>())).Do(c =>
            {
                wait.Set();
                throw new Exception();
            });
            
            var phoenix = new Phoenix(_invocation, CacheInfo);

            // Action
            phoenix.Reborn();
            Assert.IsTrue(wait.WaitOne(2000));
            wait.Reset();
            Assert.IsTrue(wait.WaitOne(2000));

            // Assert
            svc.Received(2).GetById(Arg.Any<Guid>());
        }
    }
}
