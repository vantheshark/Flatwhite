using System.Collections.Generic;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class FlatwhiteCacheEntryChangeMonitorTests
    {
        [Test]
        public void Should_dispose_after_change_and_unsubscribe_event()
        {
            var count = 0;
            var svc = new FlatwhiteCacheEntryChangeMonitor("1");
            svc.CacheMonitorChanged += x => { count ++; };
            
            Global.RevalidateCaches(new List<string> { "1", "2", "3" });
            Global.RevalidateCaches(new List<string> { "1", "2", "3" });

            Assert.AreEqual(1, count);
        }
    }
}
