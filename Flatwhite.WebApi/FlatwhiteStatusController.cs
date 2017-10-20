using Flatwhite.Hot;
using Flatwhite.Provider;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web.Http;

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
        public async Task<IHttpActionResult> Phoenixes()
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

                var asyncStore = _cacheStoreProvider.GetAsyncCacheStore(p.Value._info.StoreId);
                if (asyncStore != null)
                {
                    var unknownCacheObject = await asyncStore.GetAsync(p.Key);
                    if (unknownCacheObject is CacheItem)
                    {
                        cacheItem = unknownCacheObject as CacheItem;
                    }
                }
                if (cacheItem != null)
                {
                    var status = new CacheItemStatus(cacheItem);
                    status.Key = p.Key;
                    status.PhoenixStatus = p.Value.GetCurrentState().GetState();
                    status.Type = p.Value.GetType().Name;
                    if (cacheItem == p.Value._info)
                    {
                        status = status.CacheItemNotFound();
                    }
                    items.Add(status);
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


        [HttpGet]
        public async Task<IHttpActionResult> Phoenix(string id)
        {
            Phoenix p;
            if (!Global.Cache.PhoenixFireCage.TryGetValue(id, out p))
            {
                return NotFound();
            }

            var asyncStore = _cacheStoreProvider.GetAsyncCacheStore(p._info.StoreId);
            if (asyncStore != null)
            {
                var unknownCacheObject = await asyncStore.GetAsync(id);
                if (unknownCacheObject is CacheItem)
                {
                    var cacheItemStatus = new CacheItemStatus(unknownCacheObject as CacheItem)
                        {
                            PhoenixStatus = p.GetCurrentState().GetState()
                        };

                    return Json(cacheItemStatus,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Formatting = Formatting.Indented
                        });
                }
                
            }
            return NotFound();
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
            }

            foreach (var i in items)
            {
                if (Global.Cache.PhoenixFireCage.ContainsKey(i.Key))
                {
                    i.PhoenixStatus = Global.Cache.PhoenixFireCage[i.Key].GetCurrentState().GetState();
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
            public CacheItemStatus(CacheItem cacheItem)
            {
                Key = cacheItem.Key;
                Type = cacheItem.GetType().Name;
                CacheData = cacheItem;
                IsStale = cacheItem.IsStale();
                
                var webApiCacheItem = cacheItem as WebApiCacheItem;
                if (webApiCacheItem != null)
                {
                    Size = webApiCacheItem.Content?.Length ?? -1;
                }
                else
                {
                    Size = GetObjectSize(cacheItem.Data);
                }
            }

            public CacheItemStatus CacheItemNotFound()
            {
                Size = null;
                return this;
            }

            public string Key { get; set; }

            [JsonProperty("_type")]
            public string Type { get; set; }
            public bool? IsStale { get; set; }

            public string PhoenixStatus { get; set; }

            public CacheItem CacheData { get; set; }

            public int? Size { get; set; }
        }
#pragma warning restore 1591
    }
}
