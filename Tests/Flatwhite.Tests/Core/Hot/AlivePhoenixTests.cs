using System;
using System.Threading;
using System.Threading.Tasks;
using Flatwhite.Hot;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Hot
{
    [TestFixture]
    public class AlivePhoenixTests
    {
        [Test]
        public void Should_create_RaisingPhoenix()
        {
            var wait = new AutoResetEvent(false);
            Func<Task<IPhoenixState>> action = () =>
            {
                wait.Set();
                IPhoenixState phoenixState = new AlivePhoenix();
                return Task.FromResult(phoenixState);
            };

            var state = new AlivePhoenix();
            state.Reborn(action);
            Assert.IsTrue(wait.WaitOne(1000));
        }

        [Test]
        public void GetState_should_return_status()
        {
            var state = new AlivePhoenix();
            Assert.AreEqual("alive", state.GetState());
        }
    }
}
