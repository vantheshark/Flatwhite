using System;
using System.Threading;
using System.Threading.Tasks;
using Flatwhite.Hot;
using NUnit.Framework;

namespace Flatwhite.Tests.Core.Hot
{
    [TestFixture]
    public class InActivePhoenixTests
    {
        [Test]
        public void Should_create_RaisingPhoenix()
        {
            var wait = new AutoResetEvent(false);
            Func<Task<IPhoenixState>> action = () =>
            {
                wait.Set();
                IPhoenixState phoenixState = new InActivePhoenix();
                return Task.FromResult(phoenixState);
            };

            var state = new InActivePhoenix();
            state.Reborn(action);
            Assert.IsTrue(wait.WaitOne(1000));
        }

        [Test]
        public void GetState_should_return_status()
        {
            var state = new InActivePhoenix();
            Assert.IsTrue(state.GetState().StartsWith("inactive for "));
        }
    }
}
