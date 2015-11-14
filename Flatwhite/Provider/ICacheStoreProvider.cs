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
        ICacheStore GetCacheStore(uint storeId = 0);

        /// <summary>
        /// Get the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        IAsyncCacheStore GetAsyncCacheStore(uint storeId = 0);

        /// <summary>
        /// Register the <see cref="ICacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        void RegisterStore(ICacheStore store, uint storeId = 0);

        /// <summary>
        /// Register the <see cref="IAsyncCacheStore" />
        /// </summary>
        /// <param name="store"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        void RegisterAsyncStore(IAsyncCacheStore store, uint storeId = 0);
    }
}