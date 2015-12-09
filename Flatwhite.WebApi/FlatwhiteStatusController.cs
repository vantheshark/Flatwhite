using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Flatwhite.WebApi
{
    /// <summary>
    /// Status controller
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FlatwhiteStatusController : ApiController
    {
        /// <summary>
        /// Get all Flatwhite cache items in memory
        /// </summary>
        /// <param name="id">StoreId</param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(MaxAge = 1)]
        public async Task<List<CacheItemStatus>> Store(int id = 0)
        {
            var all = new List<KeyValuePair<string, object>>();
            

            var syncStore = Global.CacheStoreProvider.GetCacheStore(id);
            if (syncStore != null)
            {
                all.AddRange(syncStore.GetAll());
            }
            var asyncStore = Global.CacheStoreProvider.GetAsyncCacheStore(id);
            if (asyncStore != null && !(asyncStore is CacheStoreAdaptor))
            {
                all.AddRange(await asyncStore.GetAll());
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
            
            return items.OrderBy(x => x.Type).ThenBy(x => x.Age).ThenBy(x => x.Size).ThenBy(x => x.StoreId).ToList();
        }

        //http://stackoverflow.com/questions/605621/how-to-get-object-size-in-memory
        /// <summary>
        /// Calculates the lenght in bytes of an object 
        /// and returns the size 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private int GetObjectSize(object obj)
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

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Checksum { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ResponseMediaType { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string ResponseCharSet { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public uint? StaleIfError { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? IgnoreRevalidationRequest { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool AutoRefresh { get; set; }
        }
#pragma warning restore 1591
    }
}
