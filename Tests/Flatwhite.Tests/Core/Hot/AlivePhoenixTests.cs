using System;
using System.Threading;
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
            Func<IPhoenixState> action = () =>
            {
                wait.Set();
                return new AlivePhoenix();
            };

            var state = new AlivePhoenix();
            state.Reborn(action);
            Assert.IsTrue(wait.WaitOne(1000));
        }
    }
}
