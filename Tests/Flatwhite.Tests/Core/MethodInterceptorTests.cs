using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        

        [Test]
        public void Should_ignore_if_method_is_not_interceptable()
        {
            
            var attributeProvider = Substitute.For<IAttributeProvider>();
            var contextProvider = Substitute.For<IContextProvider>();
            var invocation = Substitute.For<_IInvocation>();
            invocation.Method.Returns(typeof(MethodInterceptorTests).GetMethod(nameof(Should_ignore_if_method_is_not_interceptable)));
            var interceptor = new MethodInterceptor(attributeProvider, contextProvider);

            // Action
            interceptor.Intercept(invocation);

            // Assert
            invocation.Received(1).Proceed();
            contextProvider.DidNotReceive().GetContext();
        }
    }
}
