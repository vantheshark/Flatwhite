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
        }
    }
}