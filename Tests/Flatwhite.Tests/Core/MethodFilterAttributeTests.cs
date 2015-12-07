using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class MethodFilterAttributeTests
    {
        [Test]
        public async Task OnMethodExecutingAsync_should_call_OnMethodExecuting()
        {
            var att = Substitute.ForPartsOf<MethodFilterAttribute>();

            await att.OnMethodExecutingAsync(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            });

            att.Received(1).OnMethodExecuting(Arg.Any<MethodExecutingContext>());
        }

        [Test]
        public void OnMethodExecutingAsync_should_return_error_task_if_there_is_exception()
        {
            var att = Substitute.ForPartsOf<MethodFilterAttribute>();
            att.When(x => x.OnMethodExecuting(Arg.Any<MethodExecutingContext>()))
               .Do(c => { throw new Exception("MethodExecuting exception"); });

            var t = att.OnMethodExecutingAsync(new MethodExecutingContext { InvocationContext = new Dictionary<string, object>()});
            Assert.IsTrue(t.Status == TaskStatus.Faulted);
            Assert.AreEqual("MethodExecuting exception", t.Exception.InnerExceptions[0].Message);
        }

        [Test]
        public async Task OnMethodExecutedAsync_should_call_OnMethodExecuted()
        {
            var att = Substitute.ForPartsOf<MethodFilterAttribute>();

            await att.OnMethodExecutedAsync(new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            }));

            att.Received(1).OnMethodExecuted(Arg.Any<MethodExecutedContext>());
        }

        [Test]
        public void OnMethodExecutedAsync_should_return_error_task_if_there_is_exception()
        {
            var att = Substitute.ForPartsOf<MethodFilterAttribute>();
            att.When(x => x.OnMethodExecuted(Arg.Any<MethodExecutedContext>()))
               .Do(c => { throw new Exception("MethodExecuted exception"); });

            var t = att.OnMethodExecutedAsync(new MethodExecutedContext(new MethodExecutingContext
            {
                InvocationContext = new Dictionary<string, object>()
            }));
            Assert.IsTrue(t.Status == TaskStatus.Faulted);
            Assert.AreEqual("MethodExecuted exception", t.Exception.InnerExceptions[0].Message);
        }
    }
}
