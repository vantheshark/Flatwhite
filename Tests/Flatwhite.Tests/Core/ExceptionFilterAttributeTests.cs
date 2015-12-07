using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class ExceptionFilterAttributeTests
    {
        [Test]
        public async Task OnExceptionAsync_should_call_OnException()
        {
            var context = new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            });
            var att = Substitute.ForPartsOf<ExceptionFilterAttribute>();
            
            await att.OnExceptionAsync(new MethodExceptionContext(new Exception(), context));
            att.Received(1).OnException(Arg.Any<MethodExceptionContext>());
            Assert.IsNull(context.Result);
        }

        [Test]
        public void OnExceptionAsync_should_handle_error()
        {
            var context = new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            });
            var att = Substitute.ForPartsOf<ExceptionFilterAttribute>();
            att.When(x => x.OnException(Arg.Any<MethodExceptionContext>())).Do(c => { throw new Exception("Test exception"); } );
            var t = att.OnExceptionAsync(new MethodExceptionContext(new Exception(), context));

            Assert.IsTrue(t.Status == TaskStatus.Faulted);
            Assert.AreEqual("Test exception", t.Exception.InnerExceptions[0].Message);
            Assert.IsNull(context.Result);
        }
    }
}
