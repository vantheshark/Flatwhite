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
        private readonly CacheItem _info;
        private IPhoenixState _phoenixState;
        private static readonly object PhoenixCage = new object();

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

        /// <summary>
        /// Initialize a phoenix with provided cacheDuration and staleWhileRevalidate values
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="info"></param>
        public Phoenix(_IInvocation invocation, CacheItem info)
        {
            _info = info;
            _phoenixState = _info.StaleWhileRevalidate > 0 ? (IPhoenixState)new RaisingPhoenix() : new DisposingPhoenix(DieAsync());
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
            lock (PhoenixCage)
            {
                _phoenixState = _phoenixState.Reborn(FireAsync);
            }
        }

        /// <summary>
        /// Rebuild the cache and return the new <see cref="IPhoenixState"/>
        /// </summary>
        /// <returns></returns>
        private async Task<IPhoenixState> FireAsync()
        {
            try
            {
                var target = GetTargetInstance();
                var invokedResult = await InvokeAndGetBareResult(target).ConfigureAwait(false);
                var cacheItem = await GetCacheItem(invokedResult).ConfigureAwait(false);
                if (cacheItem == null)
                {
                    var disposing = new DisposingPhoenix(DieAsync());
                    return disposing.Reborn(null);
                }

                var cacheStore = Global.CacheStoreProvider.GetAsyncCacheStore(_info.StoreId);
                //NOTE: Because the cacheItem was created before, the cacheStore cannot be null
                await cacheStore.SetAsync(_info.Key, cacheItem, DateTime.UtcNow.AddSeconds(_info.MaxAge + _info.StaleWhileRevalidate)).ConfigureAwait(false);

                WriteCacheUpdatedLog();
                _timer.Change(_info.GetRefreshTime(), TimeSpan.Zero);

                return new AlivePhoenix();
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while refreshing key {_info.Key}, store \"{_info.StoreId}\". Will retry after 1 second.", ex);
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.Zero);
                throw;
            }
        }

        /// <summary>
        /// Write cache updated log
        /// </summary>
        protected virtual void WriteCacheUpdatedLog()
        {
            Global.Logger.Info($"Updated key \"{_info.Key}\", store \"{_info.StoreId}\"");
        }

        /// <summary>
        /// Invoke the MethodInfo against the serviceInstance, then build the CacheItem object to store in cache
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <returns></returns>
        protected virtual async Task<object> InvokeAndGetBareResult(object serviceInstance)
        {
            if (MethodInfo.ReturnType == typeof (void))
            {
                throw new NotSupportedException("void method is not supported");
            }
            var isAsync = typeof(Task).IsAssignableFrom(MethodInfo.ReturnType);
            if (isAsync &&(!MethodInfo.ReturnType.IsGenericType || MethodInfo.ReturnType.GetGenericTypeDefinition() != typeof (Task<>)))
            {
                throw new NotSupportedException("async void method is not supported");
            }

            var invokeResult = MethodInfo.Invoke(serviceInstance, Arguments);
            if (invokeResult == null)
            {
                return null;
            }

            var result = invokeResult;
            
            if (isAsync)
            {
                var resultTask = (Task)invokeResult;
                await resultTask.ConfigureAwait(false);
                dynamic taskResult = resultTask;
                result = taskResult.Result;
            }

            return result;
        }

        /// <summary>
        /// Build the cache item object for the result of the method
        /// </summary>
        /// <param name="invocationBareResult"></param>
        /// <returns></returns>
        protected virtual Task<CacheItem> GetCacheItem(object invocationBareResult)
        {
            if (invocationBareResult == null)
            {
                return Task.FromResult((CacheItem)null);
            }

            return Task.FromResult(new CacheItem
            {
                CreatedTime = DateTime.UtcNow,
                Data = invocationBareResult,
                Key = _info.Key,
                MaxAge = _info.MaxAge,
                StoreId = _info.StoreId,
                StaleWhileRevalidate = _info.StaleWhileRevalidate,
                AutoRefresh = _info.AutoRefresh
            });
        }

        /// <summary>
        /// Using Activator to create an instance of the service to invoke and get result
        /// </summary>
        /// <returns></returns>
        protected virtual object GetTargetInstance()
        {
            var instance = Activator.CreateInstance(MethodInfo.DeclaringType);
            var target = _instanceTargetField != null ? _instanceTargetField.GetValue(instance) : instance;
            return target;
        }

        /// <summary>
        /// Service actvator
        /// </summary>
        protected virtual IServiceActivator Activator => Global.ServiceActivator;

        /// <summary>
        /// Dispose the timer and remove it from <see cref="Global.Cache"/>
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            Global.Cache.PhoenixFireCage.Remove(_info.Key);
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
    }
}