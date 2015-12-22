using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests
{
    [TestFixture]
    public class MethodInterceptorDemoTests
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
            Global.Logger = new ConsoleLogger();
        }

        [Test]
        public void Test_intercept_sync_method()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj.GetById(Arg.Any<Guid>()).Returns(new object());

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                var usr = interceptedSvc.GetById(Guid.Empty);
            }
            Console.WriteLine($"{nameof(Test_intercept_sync_method)} : {sw.ElapsedMilliseconds} ms");
            mockObj.Received(1).GetById(Arg.Any<Guid>());
        }

        [Test]
        public async Task Test_intercept_async_method()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new object()));

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                var usr = await interceptedSvc.GetByIdAsync(Guid.Empty).ConfigureAwait(false);
            }
            Console.WriteLine($"{nameof(Test_intercept_async_method)} : {sw.ElapsedMilliseconds} ms");
            await mockObj.Received(1).GetByIdAsync(Arg.Any<Guid>());
        }

        [Test]
        public async Task Test_intercept_async_void_method()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new object()));

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000; i++)
            {
                await interceptedSvc.DisableUserAsync(Guid.Empty).ConfigureAwait(false);
            }
            Console.WriteLine($"{nameof(Test_intercept_async_void_method)} : {sw.ElapsedMilliseconds} ms");
        }

        [Test]
        public void Should_throw_if_no_exception_filter_attribute_found()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj
                .When(x => x.GetById(Arg.Any<Guid>()))
                .Do(c => { throw new Exception(); });

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();

            Assert.Throws<Exception>(() => interceptedSvc.GetById(Guid.NewGuid()));
        }

        [Test]
        public void Test_error_handler()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj
                .When(x => x.GetByEmail(Arg.Any<string>()))
                .Do(c => { throw new Exception(); });

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();

            var obj = interceptedSvc.GetByEmail("");

            Assert.AreEqual("Error OnException", obj);
        }

        [Test]
        public void Test_error_handler_for_exception_thrown_by_other_filters()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj.GetRoles(Arg.Any<Guid>()).Returns(new List<object> {new {}, new {}});
            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();

            var obj = interceptedSvc.GetRoles(Guid.NewGuid());
            Assert.IsNotNull(obj);
            Assert.AreEqual(2, obj.Count());
        }

        [Test]
        public async Task Test_error_handler_on_async_method()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj
                .When(x => x.GetByEmailAsync(Arg.Any<string>()))
                .Do(c => { throw new Exception(); });

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();

            var obj = await interceptedSvc.GetByEmailAsync("");

            Assert.AreEqual("Error OnExceptionAsync", obj);
        }

        [Test]
        public async Task Test_error_handler_on_async_void_method()
        {
            var mockObj = Substitute.For<IUserService>();
            mockObj
                .When(x => x.DisableUserAsync(Arg.Any<Guid>()))
                .Do(c => { throw new Exception(); });

            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterInstance(mockObj)
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();

            await interceptedSvc.DisableUserAsync(Guid.NewGuid());
        }

        private class DummyUserService : IUserService
        {
            public object GetById(Guid userId)
            {
                Console.WriteLine($"{DateTime.Now}: \t\t\t\treal method is called");
                Thread.Sleep(1000); // To stimulate expensive call
                return new object();
            }

            public Task<object> GetByIdAsync(Guid userId)
            {
                throw new NotImplementedException();
            }

            public object GetByEmail(string email)
            {
                throw new NotImplementedException();
            }

            public Task<object> GetByEmailAsync(string email)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<object> GetRoles(Guid userId)
            {
                throw new NotImplementedException();
            }

            public void DisableUser(Guid userId)
            {
                throw new NotImplementedException();
            }

            public Task DisableUserAsync(Guid userId)
            {
                throw new NotImplementedException();
            }

            public object TestCustomStoreId()
            {
                throw new NotImplementedException();
            }
        }

        [Test, Explicit("This is long running test to see the method is invoked every 2 seconds")]
        public void Test_async_cache_refresh_after_stale()
        {
            Global.CacheStoreProvider.RegisterStore(new ObjectCacheStore());


            var builder = new ContainerBuilder().EnableFlatwhite();
            builder
                .RegisterType<DummyUserService>()
                .As<IUserService>()
                .EnableInterceptors();

            var container = builder.Build();

            var interceptedSvc = container.Resolve<IUserService>();
            for (var i = 0; i < 1000; i++)
            {
                var usr = interceptedSvc.GetById(Guid.Empty);
                Thread.Sleep(10);
            }
        }
    }

    public class TestMethodFilterAttribute : MethodFilterAttribute
    {
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethodFilterAttribute)} OnMethodExecuted");
        }

        public override Task OnMethodExecutedAsync(MethodExecutedContext actionExecutedContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethodFilterAttribute)} OnMethodExecutedAsync");
            return TaskHelpers.DefaultCompleted;
        }

        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethodFilterAttribute)} OnMethodExecuting");
        }

        public override Task OnMethodExecutingAsync(MethodExecutingContext actionContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethodFilterAttribute)} OnMethodExecutingAsync");
            return TaskHelpers.DefaultCompleted;
        }
    }

    public class TestMethod2FilterAttribute : MethodFilterAttribute
    {
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethod2FilterAttribute)} OnMethodExecuted");
        }

        public override Task OnMethodExecutedAsync(MethodExecutedContext actionExecutedContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethod2FilterAttribute)} OnMethodExecutedAsync");
            return TaskHelpers.DefaultCompleted;
        }

        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethod2FilterAttribute)} OnMethodExecuting");
        }

        public override Task OnMethodExecutingAsync(MethodExecutingContext actionContext)
        {
            //Console.WriteLine($"{DateTime.Now} {nameof(TestMethod2FilterAttribute)} OnMethodExecutingAsync");
            return TaskHelpers.DefaultCompleted;
        }
    }


    public class BadMethodFilterAttribute : MethodFilterAttribute
    {
        public override void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
        {
            throw new Exception();
        }

        public override Task OnMethodExecutedAsync(MethodExecutedContext actionExecutedContext)
        {
            throw new Exception();
        }

        public override void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
        {
        }

        public override Task OnMethodExecutingAsync(MethodExecutingContext actionContext)
        {
            return TaskHelpers.DefaultCompleted;
        }
    }

    public class SwallowExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(MethodExceptionContext exceptionContext)
        {
            exceptionContext.Handled = true;
            
        }

        public override Task OnExceptionAsync(MethodExceptionContext exceptionContext)
        {
            exceptionContext.Handled = true;
            return TaskHelpers.DefaultCompleted;
        }
    }

    public class TestHandleExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(MethodExceptionContext exceptionContext)
        {
            exceptionContext.Handled = true;
            exceptionContext.Result = "Error OnException";
            //Console.WriteLine($"{DateTime.Now} {nameof(MethodExceptionContext)} OnException");
        }

        public override Task OnExceptionAsync(MethodExceptionContext exceptionContext)
        {
            exceptionContext.Handled = true;
            exceptionContext.Result = "Error OnExceptionAsync";
            //Console.WriteLine($"{DateTime.Now} {nameof(MethodExceptionContext)} OnExceptionAsync");
            return TaskHelpers.DefaultCompleted;
        }
    }
}
