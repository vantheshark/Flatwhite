using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;

namespace Flatwhite.Tests
{
    /// <summary>
    /// Register this to the IOC container to override the global <see cref="ObjectCacheStore"/> which used static ObjectCache that will break the unit tests
    /// https://msdn.microsoft.com/en-us/library/system.runtime.caching.changemonitor(v=vs.110).aspx
    /// </summary>
    public class UnitTestCacheStore : ICacheStore
    {
        private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();
        public void Set(string key, object value, CacheItemPolicy policy)
        {
            _cache[key] = value;
            foreach (var mon in policy.ChangeMonitors)
            {
                // https://msdn.microsoft.com/en-us/library/system.runtime.caching.onchangedcallback(v=vs.110).aspx
                mon.NotifyOnChanged(cacheKey =>
                {
                    Remove(cacheKey as string);
                });
            }
        }

        public object Remove(string key)
        {
            if (Contains(key))
            {
                var obj = Get(key);
                _cache.Remove(key);
                return obj;
            }
            return null;
        }

        public object Get(string key)
        {
            return Contains(key) ? _cache[key] : null;
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public uint StoreId => 0;
    }

    public class FlatwhiteCacheEntryChangeMonitor : CacheEntryChangeMonitor
    {
        public FlatwhiteCacheEntryChangeMonitor(IEnumerable<string> keys)
        {
            CacheKeys = new ReadOnlyCollection<string>(keys.ToList());
            UniqueId = Guid.NewGuid().ToString("N");
            LastModified = DateTimeOffset.UtcNow;
            RegionName = "";
            InitializationComplete();
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override string UniqueId { get; }
        public override ReadOnlyCollection<string> CacheKeys { get; }
        public override DateTimeOffset LastModified { get; }
        public override string RegionName { get; }
    }
}
