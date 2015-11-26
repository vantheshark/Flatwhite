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
        private readonly CacheInfo _info;
        private IPhoenixState _phoenixState;

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
        /// <param name="state"></param>
        public Phoenix(_IInvocation invocation, CacheInfo info, object state = null)
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

            _timer = new Timer(Reborn, state, _info.GetRefreshTime(), TimeSpan.Zero);
        }
        
        /// <summary>
        /// Refresh the cache
        /// </summary>
        public virtual void Reborn(object state)
        {
            Func<IPhoenixState> rebornAction = () =>
            {
                try
                {
                    var target = GetTargetInstance();
                    var cacheItem = GetCacheItem(InvokeAndGetBareResult(target, state), state);
                    if (cacheItem == null)
                    {
                        var disposing =  new DisposingPhoenix(Die);
                        return disposing.Reborn(null);
                    }
                    
                    var cacheStore = Global.CacheStoreProvider.GetCacheStore(_info.CacheStoreId);
                    cacheStore.Set(_info.CacheKey, cacheItem, DateTime.UtcNow.AddSeconds(_info.CacheDuration + _info.StaleWhileRevalidate));

                    WriteCacheUpdatedLog();
                    _timer.Change(_info.GetRefreshTime(), TimeSpan.Zero);

                    return new AlivePhoenix();
                }
                catch (Exception ex)
                {
                    Global.Logger.Error($"Error while refreshing key {_info.CacheKey}, store \"{_info.CacheStoreId}\". Will retry after 1 second.", ex);
                    _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.Zero);
                    throw;
                }
            };

            _phoenixState = _phoenixState.Reborn(rebornAction);
        }

        /// <summary>
        /// Write cache updated log
        /// </summary>
        protected virtual void WriteCacheUpdatedLog()
        {
            Global.Logger.Info($"Updated key \"{_info.CacheKey}\", store \"{_info.CacheStoreId}\"");
        }

        /// <summary>
        /// Invoke the MethodInfo against the serviceInstance, then build the CacheItem object to store in cache
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual object InvokeAndGetBareResult(object serviceInstance, object state)
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
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual CacheItem GetCacheItem(object invocationBareResult, object state)
        {
            if (invocationBareResult == null)
            {
                return null;
            }

            return new CacheItem
            {
                CreatedTime = DateTime.UtcNow,
                Data = invocationBareResult,
                Key = _info.CacheKey,
                MaxAge = _info.CacheDuration,
                StoreId = _info.CacheStoreId,
                StaleWhileRevalidate = _info.StaleWhileRevalidate
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
            Global.Cache.PhoenixFireCage.Remove(_info.CacheKey);
        }

        private void Die()
        {
            try
            {
                var cacheStore = Global.CacheStoreProvider.GetCacheStore(_info.CacheStoreId);
                cacheStore.Remove(_info.CacheKey);
                Dispose();
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while deleting the cache key {_info.CacheKey}, store {_info.CacheStoreId}", ex);
            }
        }
    }
}