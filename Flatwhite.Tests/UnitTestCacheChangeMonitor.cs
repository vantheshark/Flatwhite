using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Flatwhite.Tests
{
    public class UnitTestCacheChangeMonitor : ChangeMonitor
    {
        public UnitTestCacheChangeMonitor()
        {
            UniqueId = Guid.NewGuid().ToString();
            InitializationComplete();
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override string UniqueId { get; }

        /// <summary>
        /// Raise the event when a change dependency changes.
        /// </summary>
        /// <param name="state"></param>
        public void FireChangeEvent(object state)
        {
            OnChanged(state);
        }
    }
}
