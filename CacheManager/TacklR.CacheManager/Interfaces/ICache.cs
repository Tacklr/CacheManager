using System;
using System.Collections.Generic;
using System.Web.Caching;

namespace TacklR.CacheManager.Interfaces
{
    internal interface ICache
    {
        int Count { get; }
        void Clear();
        void Clear(string key, bool prefix = false);
        bool Exists(string key);
        IList<string> Find(string prefix);
        object Get(string key);
        T Get<T>(string key) where T : class;
        IDictionary<string, object> GetAll(string key = null, bool prefix = false);
        IList<string> Keys();
        bool TryGet<T>(string key, out T value) where T : class;

        //Make these specific to the HttpCache? I'm not sure if this is applicable to other types of caches we may want.
        ICacheEntry GetEntry(string key);
        ICacheEntry<T> GetEntry<T>(string key) where T : class;
        IDictionary<string, ICacheEntry> GetAllEntries(string key = null, bool prefix = false);
        bool TryGetEntry<T>(string key, out ICacheEntry<T> value) where T : class;
    }
}