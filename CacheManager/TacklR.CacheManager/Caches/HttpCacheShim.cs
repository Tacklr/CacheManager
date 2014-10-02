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
            foreach (var cacheKey in Keys())
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

            static CacheInfo()
            {
                var testKey = Resources.BundleToken;
                Cache.Add(testKey, testKey, null, DateTime.UtcNow.AddMinutes(1), Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);

                var cacheEntry = CacheGet.Invoke(Cache, new object[] { testKey, 1 });
                var cacheEntryType = cacheEntry.GetType();
                UtcCreated = cacheEntryType.GetProperty("UtcCreated", BindingFlags.NonPublic | BindingFlags.Instance);
                UtcExpires = cacheEntryType.GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
                SlidingExpiration = cacheEntryType.GetProperty("SlidingExpiration", BindingFlags.NonPublic | BindingFlags.Instance);
                UsageBucket = cacheEntryType.GetProperty("UsageBucket", BindingFlags.NonPublic | BindingFlags.Instance);
                //Dependency?

                //UsageBucket === byte.MaxValue = CacheItemPriority.NotRemovable
                //else UsageBucket + 1 = (byte)CacheItemPriority.<value>

                HttpRuntime.Cache.Remove(testKey);
            }

            //Can we merge these methods?
            internal static CacheEntry GetCacheEntry(string key)
            {
                var value = Cache.Get(key);
                //check if key exists first?
                var cacheEntry = CacheGet.Invoke(HttpRuntime.Cache, new object[] { key, 1 });

                var utcCreated = UtcCreated.GetValue(cacheEntry, null) as DateTime?;
                var utcExpires = UtcExpires.GetValue(cacheEntry, null) as DateTime?;
                var slidingExpiration = SlidingExpiration.GetValue(cacheEntry, null) as TimeSpan?;
                var usageBucket = UsageBucket.GetValue(cacheEntry, null) as byte? ?? default(byte);
                var priority = usageBucket == byte.MaxValue ? CacheItemPriority.NotRemovable : (CacheItemPriority)(usageBucket + 1);

                return new CacheEntry
                {
                    Key = key,
                    Value = value,
                    AbsoluteExpiration = utcExpires.HasValue ? utcExpires.Value : Cache.NoAbsoluteExpiration,
                    SlidingExpiration = slidingExpiration.HasValue ? slidingExpiration.Value : Cache.NoSlidingExpiration,
                    Priority = priority
                };
            }

            internal static CacheEntry<T> GetCacheEntry<T>(string key)
            {
                var value = (T)Cache.Get(key);
                //check if key exists first?
                var cacheEntry = CacheGet.Invoke(HttpRuntime.Cache, new object[] { key, 1 });

                var utcCreated = UtcCreated.GetValue(cacheEntry, null) as DateTime?;
                var utcExpires = UtcExpires.GetValue(cacheEntry, null) as DateTime?;
                var slidingExpiration = SlidingExpiration.GetValue(cacheEntry, null) as TimeSpan?;
                var usageBucket = UsageBucket.GetValue(cacheEntry, null) as byte? ?? default(byte);
                var priority = usageBucket == byte.MaxValue ? CacheItemPriority.NotRemovable : (CacheItemPriority)(usageBucket + 1);



                return new CacheEntry<T>
                {
                    Key = key,
                    Value = value,
                    AbsoluteExpiration = utcExpires.HasValue ? utcExpires.Value : Cache.NoAbsoluteExpiration,
                    SlidingExpiration = slidingExpiration.HasValue ? slidingExpiration.Value : Cache.NoSlidingExpiration,
                    Priority = priority
                };
            }
        }
    }
}