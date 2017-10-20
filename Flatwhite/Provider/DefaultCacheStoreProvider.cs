using System;
using System.Collections.Generic;

namespace Flatwhite.Provider
{
    /// <summary>
    /// The default implementation of <see cref="ICacheStoreProvider" /> using private dictionaries
    /// </summary>
    public class DefaultCacheStoreProvider : ICacheStoreProvider, IDisposable
    {
        private readonly IDictionary<int, ICacheStore> _cacheStore = new Dictionary<int, ICacheStore>();
        private readonly IDictionary<int, IAsyncCacheStore> _asyncCacheStore = new Dictionary<int, IAsyncCacheStore>();
        private readonly IDictionary<Type, ICacheStore> _cacheStoreTypes = new Dictionary<Type, ICacheStore>();
        private readonly IDictionary<Type, IAsyncCacheStore> _asyncCacheStoreTypes = new Dictionary<Type, IAsyncCacheStore>();

        /// <summary>
        /// Initializes a DefaultCacheStoreProvider with default <see cref="ICacheStore" /> and <see cref="IAsyncCacheStore" />
        /// </summary>
        public DefaultCacheStoreProvider()
        {
            var objectStore = new ObjectCacheStore();
            _cacheStore[0] = objectStore;
            _cacheStoreTypes[typeof(ICacheStore)] = objectStore;
            _cacheStoreTypes[typeof(ObjectCacheStore)] = objectStore;

            var adaptor = new CacheStoreAdaptor(objectStore);
            _asyncCacheStore[0] = adaptor;
            _cacheStoreTypes[typeof(IAsyncCacheStore)] = adaptor;
        }

        /// <summary>
        /// Gets all async cache stores registered in the system
        /// </summary>
        /// <value>All async cache stores.</value>
        public ICollection<IAsyncCacheStore> AllAsyncCacheStores
        {
            get
            {
                return _asyncCacheStore.Values;
            }
        }

        /// <summary>
        /// Get the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ICacheStore GetCacheStore(int storeId)
        {
            if (storeId < 0)
            {
                return null;
            }
            if (_cacheStore.ContainsKey(storeId))
            {
                return _cacheStore[storeId];
            }
            return null;
        }

        /// <summary>
        /// Get the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="cacheStoreType"></param>
        /// <returns></returns>
        public ICacheStore GetCacheStore(Type cacheStoreType)
        {
            if (_cacheStoreTypes.ContainsKey(cacheStoreType))
            {
                return _cacheStoreTypes[cacheStoreType];
            }
            throw new KeyNotFoundException($"CacheStore {cacheStoreType.Name} not found");
        }

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" /> by storeId, if couldn't find it it will try to get the <see cref="ICacheStore" /> with the same id
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public IAsyncCacheStore GetAsyncCacheStore(int storeId)
        {
            if (storeId < 0)
            {
                return null;
            }
            if (_asyncCacheStore.ContainsKey(storeId))
            {
                return _asyncCacheStore[storeId];
            }
            if (_cacheStore.ContainsKey(storeId))
            {
                return new CacheStoreAdaptor(_cacheStore[storeId]);
            }
            return null;
        }

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="asyncCacheStoreType"></param>
        /// <returns></returns>
        public IAsyncCacheStore GetAsyncCacheStore(Type asyncCacheStoreType)
        {
            if (_asyncCacheStoreTypes.ContainsKey(asyncCacheStoreType))
            {
                return _asyncCacheStoreTypes[asyncCacheStoreType];
            }
            throw new KeyNotFoundException($"CacheStore {asyncCacheStoreType.Name} not found");
        }

        /// <summary>
        /// Register the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public void RegisterStore(ICacheStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (store.StoreId > 0 && _asyncCacheStore.ContainsKey(store.StoreId))
            {
                throw new InvalidOperationException($"There is a registered IAsyncCacheStore with id {store.StoreId}: {_asyncCacheStore[store.StoreId].GetType().Name}");
            }

            _cacheStore[store.StoreId] = store;
            _cacheStoreTypes[store.GetType()] = store;
        }

        /// <summary>
        /// Register the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public void RegisterAsyncStore(IAsyncCacheStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (store.StoreId > 0 && _cacheStore.ContainsKey(store.StoreId))
            {
                throw new InvalidOperationException($"There is a registered ICacheStore with id {store.StoreId}: {_cacheStore[store.StoreId].GetType().Name}");
            }
            _asyncCacheStore[store.StoreId] = store;
            _asyncCacheStoreTypes[store.GetType()] = store;
        }

        /// <summary>
        /// Clear all the dictionary
        /// </summary>
        public void Dispose()
        {
            _cacheStore.Clear();
            _cacheStoreTypes.Clear();
            _asyncCacheStore.Clear();
            _asyncCacheStoreTypes.Clear();
        }
    }
}