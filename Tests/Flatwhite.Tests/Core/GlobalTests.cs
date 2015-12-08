using System.Collections.Generic;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class GlobalTests
    {
        [Test, Ignore("This is hard to test as many other test created subscriptions to the event handlers")]
        public void RevalidateCachesAsync_should_return_completed_task_if_no_event_subscriptions()
        {
            var t =  Global.RevalidateCachesAsync(new List<string> {"1", "2"});
            Assert.IsTrue(t.IsCompleted);
        }
    }
}
