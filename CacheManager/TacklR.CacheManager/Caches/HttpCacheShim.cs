using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using TacklR.CacheManager.Interfaces;

namespace TacklR.CacheManager.Caches
{
    internal class HttpCacheShim : ICache
    {
        private Cache Cache
        {
            get
            {
                return HttpRuntime.Cache;
            }
        }

        public int Count
        {
            get
            {
                return Cache.Count;
            }
        }

        public void Clear()
        {
            //Combine clear methods with all optional parameters?
            Keys().ToList().ForEach(cacheKey => Cache.Remove(cacheKey));
        }

        public void Clear(string key, bool prefix = false)//pass in seperator?
        {
            if (prefix)
                Find(key).ToList().ForEach(cacheKey => Cache.Remove(cacheKey));
            else
                Cache.Remove(key);
        }

        public bool Exists(string key)
        {
            return Cache.Get(key) != null;
        }

        public IList<string> Find(string prefix)
        {
            return Keys().Where(k => k.StartsWith(prefix)).ToList();
        }

        public object Get(string key)
        {
            return Cache.Get(key);
        }

        public T Get<T>(string key) where T : class
        {
            return Cache.Get(key) as T;
        }

        //Are cache keys case sensitive?
        public IDictionary<string, object> GetAll(string prefix = null)
        {
            var checkPrefix = !String.IsNullOrEmpty(prefix);
            var cacheEntries = new Dictionary<string, object>();
            var enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (!checkPrefix || key.StartsWith(prefix))
                    cacheEntries.Add(key, enumerator.Value);
            }
            return cacheEntries;
        }

        public IList<string> Keys()
        {
            var cacheKeys = new List<string>();
            var enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                cacheKeys.Add(enumerator.Key.ToString());
            }
            return cacheKeys;
        }

        public bool TryGet<T>(string key, out T value) where T : class
        {
            try
            {
                value = Get<T>(key);
                return true;
            }
            catch (Exception)
            {
                value = default(T);
                return false;
            }
        }
    }
}