using System.Collections.Generic;
using NUnit.Framework;

namespace Flatwhite.Tests.Core
{
    [TestFixture]
    public class FlatwhiteCacheEntryChangeMonitorTests
    {
        [Test]
        public void Dispose_should_unsubscribe_event()
        {
            var count = 0;
            var svc = new FlatwhiteCacheEntryChangeMonitor("1");
            svc.CacheMonitorChanged += x => { count ++; };
            Global.RevalidateCaches(new List<string> {"1", "2", "3"});
            Global.RevalidateCaches(new List<string> { "1", "22", "33" });
            Assert.AreEqual(2, count);
            svc.Dispose();
            Global.RevalidateCaches(new List<string> { "1", "2", "3" });
            Global.RevalidateCaches(new List<string> { "1", "22", "33" });
            Assert.AreEqual(2, count);
        }
    }
}
