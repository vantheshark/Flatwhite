using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class InvocationAttributeTests
    {
        [Test]
        public void OnMethodExecuting_should_proceed_the_invocation_and_set_result()
        {
            var invocation = Substitute.For<_IInvocation>();
            invocation.ReturnValue.Returns(10);
            var context = new MethodExecutingContext
            {
                Invocation = invocation,
                InvocationContext = new Dictionary<string, object>(),
            };

            var filter = new InvocationAttribute(invocation, null);
            filter.OnMethodExecuting(context);

            // Assert
            Assert.AreEqual(10, context.Result);
            invocation.Received(1).Proceed();
        }

        [Test]
        public async Task OnMethodExecutingAsync_should_proceed_the_invocation_and_set_result()
        {
            var wait = new ManualResetEvent(false);
            var invocation = Substitute.For<_IInvocation>();
            invocation.When(x => x.Proceed()).Do(c => wait.Set());
            invocation.ReturnValue.Returns(c => Task<int>.Factory.StartNew(() =>
            {
                Assert.IsTrue(wait.WaitOne(2000));
                return 10;
            }));

            var context = new MethodExecutingContext
            {
                Invocation = invocation,
                InvocationContext = new Dictionary<string, object>(),
            };

            var filter = new InvocationAttribute(invocation, typeof(int));
            await filter.OnMethodExecutingAsync(context);

            // Assert
            Assert.AreEqual(10, context.Result);
        }

        [Test]
        public async Task OnMethodExecutingAsync_should_wait_on_async_void_TaskResult()
        {
            var wait = new ManualResetEvent(false);
            var invocation = Substitute.For<_IInvocation>();
            invocation.When(x => x.Proceed()).Do(c => wait.Set());
            Task task = null;
            invocation.ReturnValue.Returns(c =>
            {
                if (task == null)
                {
                    task = Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(100);
                        Assert.IsTrue(wait.WaitOne(2000));
                    });
                }
                return task;
            });

            var context = new MethodExecutingContext
            {
                Invocation = invocation,
                InvocationContext = new Dictionary<string, object>(),
            };

            var filter = new InvocationAttribute(invocation);
            await filter.OnMethodExecutingAsync(context);

            // Assert
            var taskResult = invocation.ReturnValue as Task;
            Assert.IsNull(context.Result);
            Assert.IsNotNull(taskResult);
            Assert.AreEqual(TaskStatus.RanToCompletion, taskResult.Status);
        }

        [Test]
        public async Task OnMethodExecutingAsync_should_proceed_and_set_result_if_the_invocation_result_is_not_Task()
        {
            var wait = new ManualResetEvent(false);
            var invocation = Substitute.For<_IInvocation>();
            invocation.When(x => x.Proceed()).Do(c => wait.Set());
            invocation.ReturnValue.Returns(c => 
            {
                Assert.IsTrue(wait.WaitOne(2000));
                return 10;
            });

            var context = new MethodExecutingContext
            {
                Invocation = invocation,
                InvocationContext = new Dictionary<string, object>(),
            };

            var filter = new InvocationAttribute(invocation, typeof(int));
            await filter.OnMethodExecutingAsync(context);

            // Assert
            Assert.AreEqual(10, context.Result);
        }
    }
}
