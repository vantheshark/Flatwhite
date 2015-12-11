using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web.Http;
using Flatwhite.Provider;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Status controller
    /// </summary>
    public class FlatwhiteStatusController : ApiController
    {
        private readonly ICacheStoreProvider _cacheStoreProvider;

        /// <summary>
        /// Initialize an instance of FlatwhiteStatusController
        /// </summary>
        /// <param name="cacheStoreProvider"></param>
        public FlatwhiteStatusController(ICacheStoreProvider cacheStoreProvider)
        {
            _cacheStoreProvider = cacheStoreProvider;
        }

        /// <summary>
        /// Initialize an instance of FlatwhiteStatusController
        /// </summary>
        [ExcludeFromCodeCoverage]
        public FlatwhiteStatusController() : this (Global.CacheStoreProvider)
        {
        }

        /// <summary>
        /// Get all phoenix statuses
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(MaxAge = 1)]
        public async Task<IHttpActionResult> Phoenix()
        {
            var all = Global.Cache.PhoenixFireCage.ToList();
            var items = new List<CacheItemStatus>();

            foreach (var p in all)
            {
                if (p.Key == null || p.Value == null)
                {
                    continue;
                }

                var cacheItem = p.Value._info;
                object unknownCacheObject = null;

                var asyncStore = _cacheStoreProvider.GetAsyncCacheStore(p.Value._info.StoreId);
                if (asyncStore != null)
                {
                    unknownCacheObject = await asyncStore.GetAsync(p.Key);
                    if (unknownCacheObject is CacheItem)
                    {
                        cacheItem = unknownCacheObject as CacheItem;
                    }
                }

                var status = cacheItem != null ? new CacheItemStatus(cacheItem) : new CacheItemStatus(unknownCacheObject);
                status.Key = p.Key;
                status.PhoenixStatus = p.Value._phoenixState.GetState();
                status.Type = p.Value.GetType().Name;
                if (cacheItem == p.Value._info)
                {
                    status = status.CacheItemNotFound();
                }
                items.Add(status);
            }

            return Json(items,
               new JsonSerializerSettings
               {
                   NullValueHandling = NullValueHandling.Ignore,
                   ContractResolver = new CamelCasePropertyNamesContractResolver(),
                   Formatting = Formatting.Indented
               });
        }

        /// <summary>
        /// Get all Flatwhite cache items in memory
        /// </summary>
        /// <param name="id">StoreId</param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(MaxAge = 1)]
        public async Task<IHttpActionResult> Store(int id = 0)
        {
            var all = new List<KeyValuePair<string, object>>();

            var syncStore = _cacheStoreProvider.GetCacheStore(id);
            if (syncStore != null)
            {
                all.AddRange(syncStore.GetAll());
            }
            var asyncStore = _cacheStoreProvider.GetAsyncCacheStore(id);
            if (asyncStore != null && !(asyncStore is CacheStoreAdaptor))
            {
                all.AddRange(await asyncStore.GetAllAsync());
            }

            var items = new List<CacheItemStatus>();
            foreach (var k in all)
            {
                var cacheItem = k.Value as CacheItem;
                if (cacheItem != null)
                {
                    items.Add(new CacheItemStatus(cacheItem));
                }
                else
                {
                    items.Add(new CacheItemStatus(k.Value) { Key = k.Key });
                }
            }

            foreach (var i in items)
            {
                if (Global.Cache.PhoenixFireCage.ContainsKey(i.Key))
                {
                    i.PhoenixStatus = Global.Cache.PhoenixFireCage[i.Key]._phoenixState.GetState();
                }
            }

            return Json(items,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                });
        }

        //http://stackoverflow.com/questions/605621/how-to-get-object-size-in-memory
        /// <summary>
        /// Calculates the lenght in bytes of an object 
        /// and returns the size 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static int GetObjectSize(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            try
            {
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    var array = ms.ToArray();
                    return array.Length;
                }
            }
            catch (SerializationException)
            {
            }

            try
            {
                return JsonConvert.SerializeObject(obj).Length;
            }
            catch
            {
            }
            return -1;
        }

#pragma warning disable 1591
        [ExcludeFromCodeCoverage]
        public class CacheItemStatus 
        {
            public CacheItemStatus()
            {
            }

            public CacheItemStatus(object unknownObject)
            {
                Type = "unknown";
                Size = GetObjectSize(unknownObject);
            }

            public CacheItemStatus(CacheItem cacheItem)
            {

                Type = cacheItem.GetType().Name;
                Key = cacheItem.Key;
                StaleWhileRevalidate = cacheItem.StaleWhileRevalidate;
                MaxAge = cacheItem.MaxAge;
                StoreId = cacheItem.StoreId;
                CreatedTime = cacheItem.CreatedTime;
                AutoRefresh = cacheItem.AutoRefresh;
                Age = cacheItem.Age;
                IsStale = cacheItem.IsStale();
                
                var webApiCacheItem = cacheItem as WebApiCacheItem;

                if (webApiCacheItem != null)
                {
                    Size = webApiCacheItem.Content?.Length ?? -1;
                    Checksum = webApiCacheItem.Checksum;
                    ResponseCharSet = webApiCacheItem.ResponseCharSet;
                    ResponseMediaType = webApiCacheItem.ResponseMediaType;
                    StaleIfError = webApiCacheItem.StaleIfError;
                    StaleWhileRevalidate = webApiCacheItem.StaleWhileRevalidate;
                }
                else
                {
                    Size = GetObjectSize(cacheItem.Data);
                }
            }

            public CacheItemStatus CacheItemNotFound()
            {
                CreatedTime = null;
                Size = null;
                StoreId = null;
                ResponseCharSet = null;
                ResponseMediaType = null;
                Age = null;
                return this;
            }

            [JsonProperty("_type")]
            public string Type { get; set; }
            public string Key { get; set; }
            public int? Size { get; set; }
            public DateTime? CreatedTime { get; set; }
            public uint? MaxAge { get; set; }
            public uint? StaleWhileRevalidate { get; set; }
            public int? StoreId { get; set; }
            public uint? Age { get; set; }
            public bool? IsStale { get; set; }
            public string Checksum { get; set; }
            public string ResponseMediaType { get; set; }
            public string ResponseCharSet { get; set; }
            public uint? StaleIfError { get; set; }
            public bool? IgnoreRevalidationRequest { get; set; }
            public bool? AutoRefresh { get; set; }
            public string PhoenixStatus { get; set; }
        }
#pragma warning restore 1591
    }
}
