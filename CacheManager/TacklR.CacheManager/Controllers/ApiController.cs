using System;
using System.Web;
using TacklR.CacheManager.Caches;
using TacklR.CacheManager.HttpHandlers;
using TacklR.CacheManager.Interfaces;
using TacklR.CacheManager.Models.Api;

namespace TacklR.CacheManager.Controllers
{
    //TODO: Attribute to control GET/POST?
    //Should every method refresh stats so they are as up-to-date as possible?
    internal class ApiController : DataHandler
    {
        internal IHttpHandler Cache()
        {
            var cache = new HttpCacheShim();
            var model = new CacheViewModel(cache) { Success = true };
            return base.Json(model);
        }

        //combine with delete? not sure how we would want to handle that
        internal IHttpHandler Clear()
        {
            var cache = new HttpCacheShim();
            cache.Clear();
            return base.Json(new ClearViewModel { Success = true });
        }

        internal IHttpHandler Combined()
        {
            var cache = new HttpCacheShim() as ICache;
            var model = new CombinedViewModel(cache, HttpContext.Current) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Delete(string key, bool prefix = false)
        {
            if (String.IsNullOrEmpty(key))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing cache key or prefix." });

            var cache = new HttpCacheShim();
            cache.Clear(key, prefix);
            return base.Json(new DeleteViewModel(cache) { Success = true });
        }

        internal IHttpHandler Details(string key)
        {
            if (String.IsNullOrEmpty(key))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing cache key." });

            var cache = new HttpCacheShim() as ICache;
            var entry = cache.GetEntry(key);
            if (entry == default(CacheEntry))//better check?
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Invalid cache key." });

            var model = new DetailsViewModel(entry) { Success = true };
            return base.Json(model);
        }

        internal IHttpHandler Page(string url)
        {
            if (String.IsNullOrEmpty(url))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Missing relative page url." });
            else if (!url.StartsWith("/"))
                return base.Json(new JsonErrorViewModel { Success = false, Message = "Url must start with a \"/\"." });

            HttpResponse.RemoveOutputCacheItem(url);
            return base.Json(new PageViewModel { Success = true });
        }

        //internal IHttpHandler Serialize(string key, bool prefix = false)
        //{
        //    //TODO: Custom serializer routing, we want to serialize the data we can serialize, instead of failing on the first bad object
        //    var cache = new HttpCacheShim() as ICache;
        //    var model = new SerializeViewModel(cache, key, prefix) { Success = true };
        //    return base.Json(model);
        //}

        //internal IHttpHandler Settings()
        //{
        //    var model = new SettingsViewModel() { Success = true };
        //    return base.Json(model);
        //}

        //internal IHttpHandler Stats()
        //{
        //    var cache = new HttpCacheShim() as ICache;
        //    var model = new StatsViewModel(cache, HttpContext.Current) { Success = true };
        //    return base.Json(model);
        //}
    }
}