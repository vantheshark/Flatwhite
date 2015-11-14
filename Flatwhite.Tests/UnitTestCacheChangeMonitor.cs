using System;
using System.Runtime.Caching;

namespace Flatwhite.Tests
{
    public class UnitTestCacheChangeMonitor : ChangeMonitor
    {
        private readonly string _cacheKey;

        public UnitTestCacheChangeMonitor(string cacheKey)
        {
            _cacheKey = cacheKey;
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
            OnChanged(_cacheKey);
        }
    }
}
