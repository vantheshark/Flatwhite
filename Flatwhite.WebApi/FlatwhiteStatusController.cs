using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        public FlatwhiteStatusController() : this (Global.CacheStoreProvider)
        {
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
                    var status = new CacheItemStatus
                    {
                        Type = cacheItem.GetType().Name,
                        Key = cacheItem.Key,
                        StaleWhileRevalidate = cacheItem.StaleWhileRevalidate,
                        MaxAge = cacheItem.MaxAge,
                        StoreId = cacheItem.StoreId,
                        CreatedTime = cacheItem.CreatedTime,
                        AutoRefresh = cacheItem.AutoRefresh,
                        Age =  cacheItem.Age,
                        IsStale = cacheItem.IsStale()
                    };

                    var webApiCacheItem = cacheItem as WebApiCacheItem;

                    if (webApiCacheItem != null)
                    {
                        status.Size = webApiCacheItem.Content.Length;
                        status.Checksum = webApiCacheItem.Checksum;
                        status.ResponseCharSet = webApiCacheItem.ResponseCharSet;
                        status.ResponseMediaType = webApiCacheItem.ResponseMediaType;
                        status.StaleIfError = webApiCacheItem.StaleIfError;
                        status.StaleWhileRevalidate = webApiCacheItem.StaleWhileRevalidate;
                    }
                    else
                    {
                        status.Size = GetObjectSize(cacheItem.Data);
                    }
                        
                    items.Add(status);
                }
                else
                {
                    items.Add(new CacheItemStatus
                    {
                        Type = "unknown",
                        Key = k.Key,
                        Size = GetObjectSize(k.Value)
                    });
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
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                var array = ms.ToArray();
                return array.Length;
            }
        }

#pragma warning disable 1591
        [ExcludeFromCodeCoverage]
        public class CacheItemStatus 
        {
            [JsonProperty("_type")]
            public string Type { get; set; }
            public string Key { get; set; }
            public int Size { get; set; }
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
        }
#pragma warning restore 1591
    }
}
