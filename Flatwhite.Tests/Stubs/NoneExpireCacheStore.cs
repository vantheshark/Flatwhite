using System.Collections.Generic;
using System.Runtime.Caching;

namespace Flatwhite.Tests.Stubs
{
    /// <summary>
    /// Register this to the IOC container to override the global <see cref="ObjectCacheStore"/> which used static ObjectCache that will break the unit tests
    /// This is a none-expire cache store
    /// https://msdn.microsoft.com/en-us/library/system.runtime.caching.changemonitor(v=vs.110).aspx
    /// </summary>
    public class NoneExpireCacheStore : ICacheStore
    {
        public NoneExpireCacheStore(uint storeId = 0)
        {
            StoreId = (int)storeId;
        }
        private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();
        public void Set(string key, object value, CacheItemPolicy policy)
        {
            _cache[key] = value;
            foreach (var mon in policy.ChangeMonitors)
            {
                // https://msdn.microsoft.com/en-us/library/system.runtime.caching.onchangedcallback(v=vs.110).aspx
                mon.NotifyOnChanged(state =>
                {
                    Remove(key);
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

        public int StoreId { get; }
    }
}
