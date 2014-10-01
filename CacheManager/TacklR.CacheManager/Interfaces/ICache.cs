using System;
using System.Collections.Generic;

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
        IDictionary<string, object> GetAll(string prefix = null);
        IList<string> Keys();
        bool TryGet<T>(string key, out T value) where T : class;
    }
}