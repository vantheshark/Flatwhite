using System;
using System.Collections.Generic;

namespace Flatwhite.Provider
{
    /// <summary>
    /// A provider to register and resolve <see cref="ICacheStore" /> or <see cref="IAsyncCacheStore" /> by id (integer)
    /// </summary>
    public interface ICacheStoreProvider
    {
        /// <summary>
        /// Get the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        ICacheStore GetCacheStore(int storeId = 0);

        /// <summary>
        /// Get the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="cacheStoreType"></param>
        /// <returns></returns>
        ICacheStore GetCacheStore(Type cacheStoreType);

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" /> by storeId, if it is not found but there is a registered <see cref="ICacheStore" /> with the same id
        /// ,the <see cref="CacheStoreAdaptor" /> will be returned instead
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IAsyncCacheStore GetAsyncCacheStore(int storeId = 0);

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="asyncCacheStoreType"></param>
        /// <returns></returns>
        IAsyncCacheStore GetAsyncCacheStore(Type asyncCacheStoreType);

        /// <summary>
        /// Register the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        void RegisterStore(ICacheStore store);


        /// <summary>
        /// Register the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        void RegisterAsyncStore(IAsyncCacheStore store);

        /// <summary>
        /// Get all available async cache stores
        /// </summary>
        ICollection<IAsyncCacheStore> AllAsyncCacheStores { get; }
    }
}