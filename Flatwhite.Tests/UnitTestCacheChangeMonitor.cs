using System;
using System.Runtime.Caching;

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
        public void FireChangeEvent()
        {
            OnChanged(null);
        }
    }
}
