using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TacklR.CacheManager.Interfaces;

namespace TacklR.CacheManager.Caches
{
    internal class ApplicationCacheShim : ICache
    {
        //I'm not even sure what the ApplicationCache is.

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Clear(string key, bool prefix = false)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string key)
        {
            throw new NotImplementedException();
        }

        public IList<string> Find(string prefix)
        {
            throw new NotImplementedException();
        }

        public object Get(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> GetAll(string key = null, bool prefix = false)
        {
            throw new NotImplementedException();
        }

        public IList<string> Keys()
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string key, out T value) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
