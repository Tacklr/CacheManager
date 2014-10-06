using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TacklR.CacheManager.Caches;
using TacklR.CacheManager.HttpHandlers;
using TacklR.CacheManager.Interfaces;
using TacklR.CacheManager.Models.Api;

namespace TacklR.CacheManager.Controllers
{
    internal class ApiController : DataHandler
    {
        internal IHttpHandler Cache()
        {
            var cache = new HttpCacheShim();
            var model = new CacheViewModel(cache) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Stats()
        {
            var cache = new HttpCacheShim() as ICache;
            var model = new StatsViewModel(cache, HttpContext.Current) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Settings()
        {
            var model = new SettingsViewModel() { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Combined(bool defer = false)//default true? should probably be a setting with a default (of?)
        {
            var cache = new HttpCacheShim() as ICache;
            var model = new CombinedViewModel(cache, HttpContext.Current, defer) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Details(string key)
        {
            var cache = new HttpCacheShim() as ICache;
            var entry = cache.GetEntry(key);//TODO: Check if null
            var model = new DetailsViewModel(entry) { Success = true };
            return base.Json(model);
        }

        //internal IHttpHandler Serialize(string key, bool prefix = false)
        //{
        //    //TODO: Custom serializer routing, we want to serialize the data we can serialize, instead of failing on the first bad object
        //    var cache = new HttpCacheShim() as ICache;
        //    var model = new SerializeViewModel(cache, key, prefix) { Success = true };
        //    return base.Json(model);
        //}

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