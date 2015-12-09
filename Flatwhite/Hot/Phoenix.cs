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
            _phoenixState = _info.StaleWhileRevalidate > 0 ? (IPhoenixState)new RaisingPhoenix() : new DisposingPhoenix(Die);
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
                _phoenixState = _phoenixState.Reborn(Fire);
            }
        }

        /// <summary>
        /// Rebuild the cache and return the new <see cref="IPhoenixState"/>
        /// </summary>
        /// <returns></returns>
        private IPhoenixState Fire()
        {
            try
            {
                var target = GetTargetInstance();
                var cacheItem = GetCacheItem(InvokeAndGetBareResult(target));
                if (cacheItem == null)
                {
                    var disposing = new DisposingPhoenix(Die);
                    return disposing.Reborn(null);
                }

                var cacheStore = Global.CacheStoreProvider.GetCacheStore(_info.StoreId);
                cacheStore.Set(_info.Key, cacheItem, DateTime.UtcNow.AddSeconds(_info.MaxAge + _info.StaleWhileRevalidate));

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
        protected virtual object InvokeAndGetBareResult(object serviceInstance)
        {
            var invokeResult = MethodInfo.Invoke(serviceInstance, Arguments);
            var result = invokeResult;
            if (result == null)
            {
                return null;
            }
            if (invokeResult is Task)
            {
                dynamic taskResult = invokeResult;
                result = taskResult.Result;
            }
            return result;
        }

        /// <summary>
        /// Build the cache item object for the result of the method
        /// </summary>
        /// <param name="invocationBareResult"></param>
        /// <returns></returns>
        protected virtual CacheItem GetCacheItem(object invocationBareResult)
        {
            if (invocationBareResult == null)
            {
                return null;
            }

            return new CacheItem
            {
                CreatedTime = DateTime.UtcNow,
                Data = invocationBareResult,
                Key = _info.Key,
                MaxAge = _info.MaxAge,
                StoreId = _info.StoreId,
                StaleWhileRevalidate = _info.StaleWhileRevalidate,
                AutoRefresh = _info.AutoRefresh
            };
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

        private void Die()
        {
            try
            {
                var cacheStore = Global.CacheStoreProvider.GetCacheStore(_info.StoreId);
                cacheStore.Remove(_info.Key);
                Dispose();
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while deleting the cache key {_info.Key}, store {_info.StoreId}", ex);
            }
        }
    }
}