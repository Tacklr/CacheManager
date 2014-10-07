using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using TacklR.CacheManager.Interfaces;

namespace TacklR.CacheManager.Caches
{
    //Static class?
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

        public long EffectivePercentagePhysicalMemoryLimit
        {
            get
            {
                return Cache.EffectivePercentagePhysicalMemoryLimit;
            }
        }

        public long EffectivePrivateBytesLimit
        {
            get
            {
                return Cache.EffectivePrivateBytesLimit;
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
        public IDictionary<string, object> GetAll(string key = null, bool prefix = false)
        {
            var cacheEntries = new Dictionary<string, object>();
            var enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())//Foreach in Keys()?
            {
                var valueKey = enumerator.Key.ToString();
                if ((!prefix && (String.IsNullOrEmpty(key) || valueKey == key)) || (prefix && valueKey.StartsWith(key)))
                    cacheEntries.Add(valueKey, enumerator.Value);
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

        //???
        public ICacheEntry GetEntry(string key)
        {
            return CacheInfo.GetCacheEntry(key);
        }

        public ICacheEntry<T> GetEntry<T>(string key) where T : class
        {
            return CacheInfo.GetCacheEntry<T>(key);
        }

        public IDictionary<string, ICacheEntry> GetAllEntries(string key = null, bool prefix = false)
        {
            var cacheEntries = new Dictionary<string, ICacheEntry>();
            var cacheKeys = Keys().Where(k => (!prefix && (String.IsNullOrEmpty(key) || k == key)) || (prefix && k.StartsWith(key)));
            foreach (var cacheKey in cacheKeys)
            {
                //Should we just die on exceptions?
                try
                {
                    cacheEntries.Add(cacheKey, CacheInfo.GetCacheEntry(cacheKey));
                }
                catch (Exception)
                {
                }
            }
            return cacheEntries;
        }

        public bool TryGetEntry<T>(string key, out ICacheEntry<T> value) where T : class
        {
            try
            {
                value = CacheInfo.GetCacheEntry<T>(key);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }







        internal static class CacheInfo
        {
            //Can we pass the Cache object in somehow?
            private static Cache Cache
            {
                get
                {
                    return HttpRuntime.Cache;
                }
            }

            private static readonly MethodInfo CacheGet = Cache.GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly PropertyInfo UtcCreated;
            private static readonly PropertyInfo UtcExpires;
            private static readonly PropertyInfo SlidingExpiration;
            private static readonly PropertyInfo UsageBucket;
            private static readonly PropertyInfo Dependency;

            static CacheInfo()
            {
                var testKey = Resources.BundleToken;//Random string instead? The main this is we don't want to collide.
                Cache.Add(testKey, testKey, null, DateTime.UtcNow.AddMinutes(1), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);

                var cacheEntry = CacheGet.Invoke(Cache, new object[] { testKey, 1 });
                var cacheEntryType = cacheEntry.GetType();
                UtcCreated = cacheEntryType.GetProperty("UtcCreated", BindingFlags.NonPublic | BindingFlags.Instance);
                UtcExpires = cacheEntryType.GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
                SlidingExpiration = cacheEntryType.GetProperty("SlidingExpiration", BindingFlags.NonPublic | BindingFlags.Instance);
                UsageBucket = cacheEntryType.GetProperty("UsageBucket", BindingFlags.NonPublic | BindingFlags.Instance);
                Dependency = cacheEntryType.GetProperty("Dependency", BindingFlags.NonPublic | BindingFlags.Instance);

                HttpRuntime.Cache.Remove(testKey);
            }

            internal static CacheEntry GetCacheEntry(string key)
            {
                //check if key exists first?
                var value = Cache.Get(key);
                var cacheEntry = CacheGet.Invoke(HttpRuntime.Cache, new object[] { key, 1 });
                if (cacheEntry == null)
                    return default(CacheEntry);

                var utcCreated = UtcCreated.GetValue(cacheEntry, null) as DateTime? ?? DateTime.MinValue;
                var utcExpires = UtcExpires.GetValue(cacheEntry, null) as DateTime? ?? Cache.NoAbsoluteExpiration;
                var slidingExpiration = SlidingExpiration.GetValue(cacheEntry, null) as TimeSpan? ?? Cache.NoSlidingExpiration;
                var usageBucket = UsageBucket.GetValue(cacheEntry, null) as byte? ?? default(byte);
                var priority = usageBucket == byte.MaxValue ? CacheItemPriority.NotRemovable : (CacheItemPriority)(usageBucket + 1);

                //var dependency = Dependency.GetValue(cacheEntry, null) as CacheDependency;

                return new CacheEntry
                {
                    Key = key,
                    Value = value,
                    Created = utcCreated,
                    AbsoluteExpiration = utcExpires,
                    SlidingExpiration = slidingExpiration,
                    Priority = priority
                };
            }

            internal static CacheEntry<T> GetCacheEntry<T>(string key)
            {
                return new CacheEntry<T>(GetCacheEntry(key));
            }
        }
    }
}