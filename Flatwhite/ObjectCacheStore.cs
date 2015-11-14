using System.Runtime.Caching;

namespace Flatwhite
{
    internal class ObjectCacheStore : ICacheStore
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        public void Set(string key, object value, CacheItemPolicy policy)
        {
            _cache.Set(key, value, policy);
        }

        public object Remove(string key)
        {
            return _cache.Remove(key);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }

        public uint StoreId => 0;
    }
}