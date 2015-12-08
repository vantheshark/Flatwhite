using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Flatwhite.AutofacIntergration;
using Flatwhite.Provider;
using Flatwhite.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class MethodInterceptorTests
    {
        [SetUp]
        public void SetUp()
        {
            Global.Init();
            Global.CacheStoreProvider.RegisterStore(new NoneExpireCacheStore());
        }

        [NoIntercept, BadMethodFilter]
        public virtual void MethodWithNoInterceptAttribute()
        {
        }

        [Test]
        public void Should_not_filter_if_method_is_not_interceptable()
        {
            
            var attributeProvider = Substitute.For<IAttributeProvider>();
            var contextProvider = Substitute.For<IContextProvider>();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(MethodInterceptorTests).GetMethod(nameof(Should_not_filter_if_method_is_not_interceptable)));
            var interceptor = new MethodInterceptor(attributeProvider, contextProvider);

            // Action
            interceptor.Intercept(invocation);

            // Assert
            invocation.Received(1).Proceed();
            contextProvider.DidNotReceive().GetContext();
        }

        [Test]
        public void Should_not_filter_if_method_has_no_intercept_attribute_decorated()
        {

            var attributeProvider = new DefaulAttributeProvider();
            var contextProvider = Substitute.For<IContextProvider>();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(MethodInterceptorTests).GetMethod(nameof(MethodWithNoInterceptAttribute)));
            var interceptor = new MethodInterceptor(attributeProvider, contextProvider);

            // Action
            interceptor.Intercept(invocation);

            // Assert
            invocation.Received(1).Proceed();
        }

        [Test]
        public void Should_throw_if_Exception_filter_throw_another_exception()
        {
            var badExceptionFilter = Substitute.For<ExceptionFilterAttribute>();
            badExceptionFilter.When(x => x.OnException(Arg.Any<MethodExceptionContext>())).Do(
                c => { throw new Exception("Thrown from execption filter"); });

            var attributeProvider = Substitute.For<IAttributeProvider>();
            attributeProvider.GetAttributes(Arg.Any<MethodInfo>(), Arg.Any<IDictionary<string, object>>())
                .Returns(new List<Attribute> {new BadMethodFilterAttribute(), badExceptionFilter});

            var contextProvider = Substitute.For<IContextProvider>();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(MethodInterceptorTests).GetMethod(nameof(MethodWithNoInterceptAttribute)));
            var interceptor = new MethodInterceptor(attributeProvider, contextProvider);

            // Action & Assert
            Assert.Throws<AggregateException>(() => interceptor.Intercept(invocation));
        }

        [Test]
        public void Should_throw_if_Exception_filter_does_not_set_hanled_to_throw()
        {
            var badExceptionFilter = Substitute.For<ExceptionFilterAttribute>();

            var attributeProvider = Substitute.For<IAttributeProvider>();
            attributeProvider.GetAttributes(Arg.Any<MethodInfo>(), Arg.Any<IDictionary<string, object>>())
                .Returns(new List<Attribute> { new BadMethodFilterAttribute(), badExceptionFilter });

            var contextProvider = Substitute.For<IContextProvider>();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(MethodInterceptorTests).GetMethod(nameof(MethodWithNoInterceptAttribute)));
            var interceptor = new MethodInterceptor(attributeProvider, contextProvider);

            // Action & Assert
            Assert.Throws<Exception>(() => interceptor.Intercept(invocation));
        }
    }
}
