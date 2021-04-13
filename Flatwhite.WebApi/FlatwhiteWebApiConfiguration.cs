using Flatwhite.WebApi.CacheControl;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Flatwhite webapi configuration
    /// </summary>
    public class FlatwhiteWebApiConfiguration
    {
        /// <summary>
        /// If true, Flatwhite will register route /_flatwhite/store/{storeId} to allow checking cache item status
        /// </summary>
        public bool EnableStatusController { get; set; }

        /// <summary>
        /// Loopback address that webapi uses to refresh the stale cache. If not set, Flatwhite will use the original request uri when attempt to refresh the cache
        /// <para>Set it to web server loopback address if server is behind firewall, example: http://localhost:port</para>
        /// </summary>
        public string LoopbackAddress { get; set; }

        /// <summary>
        /// Initialize  default flatwhite webapi config
        /// </summary>
        public FlatwhiteWebApiConfiguration()
        {
            EnableStatusController = true;
            ResponseBuilder = new CacheResponseBuilder();
            IgnoreVaryCustomKeys = false;
        }

        /// <summary>
        /// Default cache response builder
        /// </summary>
        public ICacheResponseBuilder ResponseBuilder { get; set; }

        /// <summary>
        /// If you dont use vary by headers and vary by custom, Flatwhite cache will try to match the cache by keys generated from the request Uri
        /// <para>So if there is an available cache, it will return the response straight away and improve the performance significantly as it doesn't need to wait until the Controller and OutputCache action filter to be created.
        /// However every (normally GET) requests come to the server will make the cache store check the cache key which could be the issue.
        /// You can override <see cref="EvaluateServerCacheHandler"/> to make it smarter by avoid the known requests that you don't need checking the cache and register to WebApi DependencyResolver
        /// </para>
        /// </summary>
        public bool IgnoreVaryCustomKeys { get; set; }
    }
}