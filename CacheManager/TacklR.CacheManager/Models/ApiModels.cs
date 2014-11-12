using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using TacklR.CacheManager.Caches;
using TacklR.CacheManager.Interfaces;

namespace TacklR.CacheManager.Models.Api
{
    abstract class BaseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    internal class JsonErrorViewModel : BaseViewModel
    {
    }

    internal class DeleteViewModel : BaseViewModel
    {
        internal DeleteViewModel(ICache cache)
        {
            Count = cache.Count;
        }

        public int Count { get; set; }
    }

    internal class ClearViewModel : BaseViewModel
    {
    }

    internal class PageViewModel : BaseViewModel
    {
    }

    //internal class SerializeViewModel : BaseViewModel
    //{
    //    internal SerializeViewModel(ICache cache, string key, bool prefix)
    //    {
    //        Key = key;
    //        Values = cache.GetAll(key, prefix);//custom serializer?
    //    }

    //    public string Key { get; set; }
    //    public IDictionary<string, object> Values { get; set; }
    //}

    internal class DetailsViewModel : BaseViewModel
    {
        internal DetailsViewModel(ICacheEntry entry)
        {
            var type = entry.Value.GetType();//null value? possible?

            Key = entry.Key;
            Type = type.FullName;//Would getting the type's assembly be useful?
            AbsoluteExpiration = (entry.AbsoluteExpiration == Cache.NoAbsoluteExpiration ? default(DateTime?) : entry.AbsoluteExpiration).ToUnixMilliseconds();
            Created = entry.Created.ToUnixMilliseconds();
            Priority = entry.Priority;
            SlidingExpiration = (entry.SlidingExpiration == Cache.NoSlidingExpiration ? default(TimeSpan?) : entry.SlidingExpiration).ToMilliseconds();

            if (type.Namespace == "System.Data.Entity.DynamicProxies")
            {
                ValueError = "Dynamic proxy objects cannot currently be serialized.";
            }
            else
            {
                try
                {
                    Value = JsonConvert.SerializeObject(entry.Value);
                }
                catch (Exception ex)
                {
                    //get inner exception message? will there ever be any?
                    ValueError = "Error serializing data." + (String.IsNullOrEmpty(ex.Message) ? String.Empty : " " + ex.Message);
                }
            }
        }

        public string Key { get; set; }
        public string Type { get; set; }
        public long? AbsoluteExpiration { get; set; }
        public long Created { get; set; }
        public long? SlidingExpiration { get; set; }
        public CacheItemPriority Priority { get; set; }

        public string Value { get; set; }
        public string ValueError { get; set; }
    }

    internal class CombinedViewModel : BaseViewModel
    {
        internal CombinedViewModel(ICache cache, HttpContext context)
        {
            Delimiter = Configuration.Delimiter;
            DetailView = Configuration.DetailView;
            ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
            ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
            ExpandSingleBranches = Configuration.ExpandSingleBranches;

            Count = cache.Count;
            MemoryFree = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.

            //Will other shims have this?
            if (cache is HttpCacheShim)
            {
                var httpCache = cache as HttpCacheShim;
                MemoryLimitPercent = httpCache.EffectivePercentagePhysicalMemoryLimit;
                var memoryLimitKB = httpCache.EffectivePrivateBytesLimit;
                MemoryLimit = memoryLimitKB == -1 ? -1 : memoryLimitKB / 1024f;
            }

            Entries =  DetailView == ViewType.Defer ? new List<Entry>() : cache.GetAll().Select(e => new Entry(e)).ToList();
        }

        public string Delimiter { get; set; }
        public ViewType DetailView { get; set; }
        public bool ConfirmDeleteKey { get; set; }
        public bool ConfirmDeletePrefix { get; set; }
        public bool ExpandSingleBranches { get; set; }

        public int Count { get; set; }
        public float? MemoryFree { get; set; }
        public float? MemoryLimit { get; set; }
        public long? MemoryLimitPercent { get; set; }

        public IList<Entry> Entries { get; set; }

        internal class Entry
        {
            internal Entry(KeyValuePair<string, object> entry)
            {
                Key = entry.Key;
                Type = entry.Value.GetType().ToString();
            }

            public string Key { get; set; }
            public string Type { get; set; }
        }
    }

    internal class CacheViewModel : BaseViewModel
    {
        internal CacheViewModel(ICache cache)
        {
            //Refresh other stats?
            Count = cache.Count;
            Entries = cache.GetAll().Select(e => new Entry(e)).ToList();
        }

        public int Count { get; set; }
        public IList<Entry> Entries { get; set; }

        internal class Entry
        {
            internal Entry(KeyValuePair<string, object> entry)
            {
                Key = entry.Key;
                Type = entry.Value.GetType().ToString();
            }

            public string Key { get; set; }
            public string Type { get; set; }
        }
    }

    internal class StatsViewModel : BaseViewModel
    {
        internal StatsViewModel(ICache cache, HttpContext context)
        {
            Count = cache.Count;
            MemoryFree = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.

            //Will other shims have this?
            if (cache is HttpCacheShim)
            {
                var httpCache = cache as HttpCacheShim;
                MemoryLimitPercent = httpCache.EffectivePercentagePhysicalMemoryLimit;
                var memoryLimitKB = httpCache.EffectivePrivateBytesLimit;
                MemoryLimit = memoryLimitKB == -1 ? -1 : memoryLimitKB / 1024f;
            }
        }

        public int Count { get; set; }
        public float? MemoryFree { get; set; }
        public float? MemoryLimit { get; set; }
        public long? MemoryLimitPercent { get; set; }
    }

    internal class SettingsViewModel : BaseViewModel
    {
        internal SettingsViewModel()
        {
            Delimiter = Configuration.Delimiter;
            DetailView = Configuration.DetailView;
            ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
            ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
            ExpandSingleBranches = Configuration.ExpandSingleBranches;
        }

        public string Delimiter { get; set; }
        public ViewType DetailView { get; set; }
        public bool ConfirmDeleteKey { get; set; }
        public bool ConfirmDeletePrefix { get; set; }
        public bool ExpandSingleBranches { get; set; }
    }

    //internal class CombinedViewModel : BaseViewModel
    //{
    //    internal CombinedViewModel(ICache cache, HttpContext context)
    //    {
    //        Delimiter = Configuration.Delimiter;
    //        ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
    //        ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
    //        ExpandSingleBranches = Configuration.ExpandSingleBranches;

    //        AppPath = context.Request.ApplicationPath;
    //        Count = cache.Count;
    //        MemoryFree = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.
    //        ServerName = Environment.MachineName;

    //        //Will other shims have this?
    //        if (cache is HttpCacheShim)
    //        {
    //            var httpCache = cache as HttpCacheShim;
    //            MemoryLimitPercent = httpCache.EffectivePercentagePhysicalMemoryLimit;
    //            var memoryLimitKB = httpCache.EffectivePrivateBytesLimit;
    //            MemoryLimit = memoryLimitKB == -1 ? -1 : memoryLimitKB / 1024f;
    //        }

    //        Entries = cache.GetAllEntries().Select(e => new Entry(e)).ToList();
    //    }

    //    public string Delimiter { get; set; }
    //    public bool ConfirmDeleteKey { get; set; }
    //    public bool ConfirmDeletePrefix { get; set; }
    //    public bool ExpandSingleBranches { get; set; }

    //    public string AppPath { get; set; }
    //    public int Count { get; set; }
    //    public float? MemoryFree { get; set; }
    //    public string ServerName { get; set; }

    //    public float? MemoryLimit { get; set; }
    //    public long? MemoryLimitPercent { get; set; }

    //    public IList<Entry> Entries { get; set; }

    //    internal class Entry
    //    {
    //        internal Entry(KeyValuePair<string, ICacheEntry> entry)
    //        {
    //            var value = entry.Value;
    //            Key = value.Key;
    //            Type = value.Value.GetType().ToString();

    //            AbsoluteExpiration = (value.AbsoluteExpiration == Cache.NoAbsoluteExpiration ? default(DateTime?) : value.AbsoluteExpiration).ToUnixMilliseconds();
    //            Created = value.Created.ToUnixMilliseconds();
    //            Priority = value.Priority;
    //            SlidingExpiration = (value.SlidingExpiration == Cache.NoSlidingExpiration ? default(TimeSpan?) : value.SlidingExpiration).ToMilliseconds();
    //        }

    //        public string Key { get; set; }
    //        public string Type { get; set; }

    //        public long? AbsoluteExpiration { get; set; }
    //        public long Created { get; set; }
    //        public long? SlidingExpiration { get; set; }
    //        public CacheItemPriority Priority { get; set; }
    //    }
    //}

    //internal class CacheViewModel : BaseViewModel
    //{
    //    internal CacheViewModel(ICache cache)
    //    {
    //        Entries = cache.GetAllEntries().Select(e => new Entry(e)).ToList();
    //    }

    //    public IList<Entry> Entries { get; set; }

    //    internal class Entry
    //    {
    //        internal Entry(KeyValuePair<string, ICacheEntry> entry)
    //        {
    //            var value = entry.Value;
    //            Key = value.Key;
    //            Type = value.Value.GetType().ToString();

    //            AbsoluteExpiration = (value.AbsoluteExpiration == Cache.NoAbsoluteExpiration ? default(DateTime?) : value.AbsoluteExpiration).ToUnixMilliseconds();
    //            Created = value.Created.ToUnixMilliseconds();
    //            Priority = value.Priority;
    //            SlidingExpiration = (value.SlidingExpiration == Cache.NoSlidingExpiration ? default(TimeSpan?) : value.SlidingExpiration).ToMilliseconds();
    //        }

    //        public string Key { get; set; }
    //        public string Type { get; set; }

    //        public long? AbsoluteExpiration { get; set; }
    //        public long Created { get; set; }
    //        public long? SlidingExpiration { get; set; }
    //        public CacheItemPriority Priority { get; set; }
    //    }
    //}
}
