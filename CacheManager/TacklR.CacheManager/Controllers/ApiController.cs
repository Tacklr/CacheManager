using System;
using System.Linq;
using System.Web;
using TacklR.CacheManager.Caches;
using TacklR.CacheManager.HttpHandlers;
using TacklR.CacheManager.Models.Api;

namespace TacklR.CacheManager.Controllers
{
    internal class ApiController : DataHandler
    {
        internal IHttpHandler Cache()
        {
            var cache = new HttpCacheShim();
            var cacheEntries = cache.GetAll();
            var model = new CacheViewModel(cacheEntries) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Stats()
        {
            var cache = new HttpCacheShim();
            var model = new StatsViewModel(cache.Count, HttpContext.Current) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Settings()
        {
            var model = new SettingsViewModel() { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Combined()
        {
            var cache = new HttpCacheShim();
            var cacheEntries = cache.GetAll();
            var model = new CombinedViewModel(cache.Count, HttpContext.Current, cacheEntries) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Serialize(string key, bool prefix = false)
        {
            //TODO: Custom serializer routing, we want to serialize the data we can serialize, instead of failing on the first bad object
            var cache = new HttpCacheShim();
            if (prefix)
                return base.Json(cache.GetAll(key));
            else
                return base.Json(cache.Get(key));//do we want just the data, or a single entry array?
        }

        //POST
        internal IHttpHandler Delete(string key, bool prefix = false)
        {
            if (String.IsNullOrEmpty(key))
                return base.Json(new { Success = false });//return error

            var cache = new HttpCacheShim();
            cache.Clear(key, prefix);
            return base.Json(new { Success = true });
        }





    }
}