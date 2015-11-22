using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flatwhite
{
    internal class CacheStoreAdaptor : ICacheStore, IAsyncCacheStore
    {
        private readonly ICacheStore _syncCacheStore;

        public CacheStoreAdaptor(ICacheStore syncCacheStore)
        {
            _syncCacheStore = syncCacheStore;
        }

        public void Set(string key, object value, CacheItemPolicy policy)
        {
            _syncCacheStore.Set(key, value, policy);
        }

        public object Remove(string key)
        {
            return _syncCacheStore.Remove(key);
        }

        public object Get(string key)
        {
            return _syncCacheStore.Get(key);
        }

        public bool Contains(string key)
        {
            return _syncCacheStore.Contains(key);
        }

        public int StoreId => _syncCacheStore.StoreId;

        public Task SetAsync(string key, object value, CacheItemPolicy policy)
        {
            _syncCacheStore.Set(key, value, policy);
            return Task.FromResult(key);
        }

        public Task<object> RemoveAsync(string key)
        {
            return Task.FromResult(_syncCacheStore.Remove(key));
        }

        public Task<object> GetAsync(string key)
        {
            return Task.FromResult(_syncCacheStore.Get(key));
        }

        public Task<bool> ContainsAsync(string key)
        {
            return Task.FromResult(_syncCacheStore.Contains(key));
        }
    }
}
