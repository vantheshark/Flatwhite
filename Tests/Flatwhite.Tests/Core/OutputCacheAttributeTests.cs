using System;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class OutputCacheAttributeTests
    {
        [Test]
        public void Should_attempt_to_refresh_cache_when_cache_is_stale()
        {
            // Arrange
            Global.Init();
            var wait = new AutoResetEvent(false);
            var userService = Substitute.For<IUserService>();
            userService.When(x => x.GetById(Arg.Any<Guid>())).Do(c => { wait.Set(); });
            var activator = Substitute.For<IServiceActivator>();
            activator.CreateInstance(Arg.Any<Type>()).Returns(userService);

            Global.ServiceActivator = activator;
            var cacheStore = Substitute.For<ICacheStore>();
            cacheStore.StoreId.Returns(10);
            cacheStore.Get(Arg.Any<string>()).Returns(c => new CacheItem
            {
                StaleWhileRevalidate = 5,
                StoreId = 10,
                MaxAge = 5,
                CreatedTime = DateTime.UtcNow.AddSeconds(-6),
                Key = c.Arg<string>(),
                Data = "Cache content"
            });
            Global.CacheStoreProvider.RegisterStore(cacheStore);
            var att = new OutputCacheAttribute {CacheStoreId = cacheStore.StoreId, Duration = 5, StaleWhileRevalidate = 5};

            
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof (IUserService).GetMethod(nameof(IUserService.GetById)));
            invocation.Arguments.Returns(new object[] {Guid.NewGuid()});

            var context = new Dictionary<string, object>
            {
                [Global.__flatwhite_outputcache_attribute] = new OutputCacheAttribute
                {
                    CacheStoreId = 100
                }
            };
            var executingContext = new MethodExecutingContext
            {
                Invocation =  invocation,
                InvocationContext = context,
                MethodInfo = invocation.Method
            };

            // Action
            att.OnMethodExecuting(executingContext);
            Assert.IsTrue(wait.WaitOne(2000));
        }
    }
}
