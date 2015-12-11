using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Flatwhite.Hot
{
    /// <summary>
    /// A class contains all the info to create the new fresh cache item when cache is about to expire
    /// </summary>
    public class Phoenix : IDisposable
    {
        /// <summary>
        /// Get information about the cache
        /// </summary>
        /// <returns></returns>
        protected internal readonly CacheItem _info;

        /// <summary>
        /// Phoenix state
        /// </summary>
        protected internal IPhoenixState _phoenixState;

        private static readonly object _phoenixCage = new object();

        /// <summary>
        /// The timer to refresh the cache item. It will run every "Duration" seconds if "StaleWhileRevalidate" > 0
        /// </summary>
        private readonly Timer _timer;
        /// <summary>
        /// The service type is properly registered using IOC container with interceptors enabled
        /// So the resolved instance at run-time is a proxy object.
        /// This is the fieldInfo of the proxy objecdt to get the read __target instance to by-passs the interceptors
        /// </summary>
        private readonly FieldInfo _instanceTargetField;

        /// <summary>
        /// The method info
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// The arguments required to invoke the MethodInfo
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object[] Arguments { get; set; }

        private bool _isAsync;

        /// <summary>
        /// Initialize a phoenix with provided cacheDuration and staleWhileRevalidate values
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="info"></param>
        public Phoenix(_IInvocation invocation, CacheItem info)
        {
            _info = info.CloneWithoutData();
            _phoenixState = _info.StaleWhileRevalidate > 0 ? (IPhoenixState)new InActivePhoenix() : new DisposingPhoenix(DieAsync());
            if (invocation.Proxy != null)
            { 
                // It is really a dynamic proxy
                _instanceTargetField = invocation.Proxy.GetType().GetField("__target", BindingFlags.Public | BindingFlags.Instance);
            }

            Arguments = invocation.Arguments;

            MethodInfo = invocation.Method;
            _timer = new Timer(_ => Reborn(), null, _info.GetRefreshTime(), TimeSpan.Zero);
        }

        /// <summary>
        /// Refresh the cache and change the internal <see cref="IPhoenixState"/> to avoid refreshing too many unnecessary times
        /// <para>The call will happen in background so the caller will not have to wait</para>
        /// </summary>
        public virtual void Reborn()
        {
            lock (_phoenixCage)
            {
                _phoenixState = _phoenixState.Reborn(FireAsync);
            }
        }

        /// <summary>
        /// Rebuild the cache and return the new <see cref="IPhoenixState"/>
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<IPhoenixState> FireAsync()
        {
            MethodInfo.CheckMethodForCacheSupported(out _isAsync);

            try
            {
                var target = GetTargetInstance();
                var invokedResult = await InvokeAndGetBareResult(target).ConfigureAwait(false);
                var cacheItem = GetCacheItem(invokedResult);
                if (cacheItem == null)
                {
                    var disposing = new DisposingPhoenix(DieAsync());
                    return disposing.Reborn(null);
                }

                var cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(_info.StoreId);
                //NOTE: Because the cacheItem was created before, the cacheStore cannot be null
                await cacheStore.SetAsync(_info.Key, cacheItem, DateTime.UtcNow.AddSeconds(_info.MaxAge + _info.StaleWhileRevalidate)).ConfigureAwait(false);

                Global.Logger.Info($"Updated key \"{_info.Key}\", store \"{_info.StoreId}\"");

                Retry(_info.GetRefreshTime());
                _phoenixState = new InActivePhoenix();
                return _phoenixState;
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while refreshing key {_info.Key}, store \"{_info.StoreId}\". Will retry after 1 second.", ex);
                Retry(TimeSpan.FromSeconds(1));
                throw;
            }
        }

        /// <summary>
        /// Invoke the MethodInfo against the serviceInstance, then build the CacheItem object to store in cache
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual async Task<object> InvokeAndGetBareResult(object serviceInstance)
        {
            var invokeResult = MethodInfo.Invoke(serviceInstance, Arguments);
            if (invokeResult == null)
            {
                return null;
            }

            var result = invokeResult;
            
            if (_isAsync)
            {
                var task = (Task) invokeResult;
                return await task.TryGetTaskResult();
            }

            return result;
        }

        /// <summary>
        /// Build the cache item object for the result of the method
        /// </summary>
        /// <param name="invocationBareResult"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual CacheItem GetCacheItem(object invocationBareResult)
        {
            if (invocationBareResult == null)
            {
                return null;
            }
            

            var newCacheItem = _info.CloneWithoutData();
            newCacheItem.CreatedTime = DateTime.UtcNow;
            newCacheItem.Data = invocationBareResult;

            return newCacheItem;
        }

        /// <summary>
        /// Using Activator to create an instance of the service to invoke and get result
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual object GetTargetInstance()
        {
            var instance = Activator.CreateInstance(MethodInfo.DeclaringType);
            var target = _instanceTargetField != null ? _instanceTargetField.GetValue(instance) : instance;
            return target;
        }

        /// <summary>
        /// Service actvator
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual IServiceActivator Activator => Global.ServiceActivator;

        private volatile bool _disposed;
        /// <summary>
        /// Dispose the timer and remove it from <see cref="Global.Cache"/>
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _timer?.Dispose();
                Global.Cache.PhoenixFireCage.Remove(_info.Key);
            }
        }

        private async Task DieAsync()
        {
            try
            {
                var cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(_info.StoreId);
                await cacheStore.RemoveAsync(_info.Key).ConfigureAwait(false);
                Dispose();
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while deleting the cache key {_info.Key}, store {_info.StoreId}", ex);
            }
        }

        /// <summary>
        /// Retry phoenix reborn after a timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        protected void Retry(TimeSpan timeSpan)
        {
            if (!_disposed)
            {
                _timer.Change(timeSpan, TimeSpan.Zero);
            }
        }
    }
}