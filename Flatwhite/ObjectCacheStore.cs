using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Caching;

namespace Flatwhite
{
    [ExcludeFromCodeCoverage]
    internal class ObjectCacheStore : ICacheStore
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        public void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            _cache.Set(key, value, absoluteExpiration);
        }

        public object Remove(string key)
        {
            return _cache.Remove(key);
        }

        public object Get(string key)
        {
            return _cache.Get(key);
        }

        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }

        public int StoreId => 0;
        public List<KeyValuePair<string, object>> GetAll()
        {
            return _cache.ToList();
        }
    }
}