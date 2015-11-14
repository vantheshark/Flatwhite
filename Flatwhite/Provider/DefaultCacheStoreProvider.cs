using System;
using System.Collections.Generic;


namespace Flatwhite.Provider
{
    /// <summary>
    /// The default implementation of <see cref="ICacheStoreProvider" /> using private dictionaries
    /// </summary>
    public class DefaultCacheStoreProvider : ICacheStoreProvider, IDisposable
    {
        private readonly IDictionary<uint, ICacheStore> _cacheStore = new Dictionary<uint, ICacheStore>();
        private readonly IDictionary<uint, IAsyncCacheStore> _asyncCacheStore = new Dictionary<uint, IAsyncCacheStore>();

        /// <summary>
        /// Get the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ICacheStore GetCacheStore(uint storeId)
        {
            if (_cacheStore.ContainsKey(storeId))
            {
                return _cacheStore[storeId];
            }
            throw new KeyNotFoundException($"Cachestore with {storeId} not found");
        }

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" /> by storeId, if couldn't find it it will try to get the <see cref="ICacheStore" /> with the same id
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public IAsyncCacheStore GetAsyncCacheStore(uint storeId)
        {
            if (_asyncCacheStore.ContainsKey(storeId))
            {
                return _asyncCacheStore[storeId];
            }
            if (_cacheStore.ContainsKey(storeId))
            {
                return new CacheStoreAdaptor(_cacheStore[storeId]);
            }
            throw new KeyNotFoundException($"Acync Cachestore with {storeId} not found");
        }


        /// <summary>
        /// Register the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public void RegisterStore(ICacheStore store, uint storeId)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
          
            _cacheStore[storeId] = store;
        }

        /// <summary>
        /// Register the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public void RegisterAsyncStore(IAsyncCacheStore store, uint storeId)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            _asyncCacheStore[storeId] = store;
        }

        /// <summary>
        /// Clear all the dictionary
        /// </summary>
        public void Dispose()
        {
            _cacheStore.Clear();
            _asyncCacheStore.Clear();
        }
    }
}