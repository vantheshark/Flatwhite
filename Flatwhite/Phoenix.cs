using System;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Flatwhite
{
    /// <summary>
    /// A class contains all the info to create the new fresh cache item when cache is about to expire
    /// </summary>
    public class Phoenix : IDisposable
    {
        private readonly string _cacheKey;
        private readonly int _cacheDuration;
        private readonly int _staleWhileRevalidate;
        /// <summary>
        /// The timer to refresh the cache item. It will run every "Duration" miliseconds if "StaleWhileRevalidate" > 0
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
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// The arguments required to invoke the MethodInfo
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// The id of the cache store that was used to keep the cache item
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Initialize a phoenix with provided cacheDuration and staleWhileRevalidate values
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="cacheStoreId"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="staleWhileRevalidate"></param>
        /// <param name="state"></param>
        public Phoenix(_IInvocation invocation, int cacheStoreId, string cacheKey, int cacheDuration, int staleWhileRevalidate, object state = null)
        {
            _cacheKey = cacheKey;
            _cacheDuration = cacheDuration;
            _staleWhileRevalidate = staleWhileRevalidate;
            if (invocation.Proxy != null)
            { 
                // It is really a dynamic proxy
                _instanceTargetField = invocation.Proxy.GetType().GetField("__target", BindingFlags.Public | BindingFlags.Instance);
            }

            Arguments = invocation.Arguments;
            StoreId = cacheStoreId;
            MethodInfo = invocation.Method;

            _timer = new Timer(Reborn, state, GetNextReborn(), TimeSpan.Zero);
        }

        private TimeSpan GetNextReborn()
        {
            return _staleWhileRevalidate > 0 ? TimeSpan.FromMilliseconds(_cacheDuration) : Timeout.InfiniteTimeSpan;
        }

        /// <summary>
        /// Refresh the cache
        /// </summary>
        public virtual void Reborn(object state = null)
        {
            try
            {
                var target = GetTargetInstance();
                var result = GetMethodResult(target, state);
                if (result == null)
                {
                    DieForever();
                    return;
                }

                Global.Logger.Info($"Refreshing cache item {_cacheKey} on cacheStore {StoreId}");
                var cacheStore = Global.CacheStoreProvider.GetCacheStore(StoreId);

                cacheStore.Set(_cacheKey, result, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddMilliseconds(_cacheDuration + _staleWhileRevalidate) });
                _timer.Change(GetNextReborn(), TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while refreshing the cache key {_cacheKey}, store {StoreId}. Will retry after 1 second.", ex);
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Invoke the MethodInfo against the serviceInstance
        /// </summary>
        /// <param name="serviceInstance"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual object GetMethodResult(object serviceInstance, object state)
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
        /// Using Activator to create an instance of the service to invoke and get result
        /// </summary>
        /// <returns></returns>
        protected virtual object GetTargetInstance()
        {
            var instance = Activator.CreateInstance(MethodInfo.DeclaringType);
            var target = _instanceTargetField != null ? _instanceTargetField.GetValue(instance) : instance;
            return target;
        }

        private void DieForever()
        {
            try
            {
                var cacheStore = Global.CacheStoreProvider.GetCacheStore(StoreId);
                _timer.Dispose();
                cacheStore.Remove(_cacheKey);
            }
            catch (Exception ex)
            {
                Global.Logger.Error($"Error while deleting the cache key {_cacheKey}, store {StoreId}", ex);
            }
        }

        /// <summary>
        /// Service actvator
        /// </summary>
        protected virtual IServiceActivator Activator => Global.ServiceActivator;

        /// <summary>
        /// Remove the cache and dispose
        /// </summary>
        public void RebornOrDieForever()
        {
            if (_staleWhileRevalidate > 0)
            {
                Reborn();
            }
            else
            {
                DieForever();
            }
        }

        /// <summary>
        /// Dispose the timer and the object
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}