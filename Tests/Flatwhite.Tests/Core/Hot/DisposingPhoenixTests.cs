using System.Threading;
using System.Threading.Tasks;
using Flatwhite.Hot;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Hot
{
    [TestFixture]
    public class DisposingPhoenixTests
    {
        [Test]
        public void Should_run_the_disposing_task_once()
        {
            var wait = new AutoResetEvent(false);
            var count = 0;
            Task action = Task.Run(() =>
            {
                wait.Set();
                count++;
            });

            var state = new DisposingPhoenix(action);
            for (var i = 0; i < 1000; i++)
            {
                Assert.AreSame(state, state.Reborn(null));
            }
            Assert.IsTrue(wait.WaitOne(1000));
            Assert.AreEqual(1, count);
        }
    }
}
