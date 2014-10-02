using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

    internal class SerializeViewModel : BaseViewModel
    {
        internal SerializeViewModel(string key, IDictionary<string, object> data)
        {
            Key = key;
            Values = data;//custom serializer?
        }

        public string Key { get; set; }
        public IDictionary<string, object> Values { get; set; }
    }

    internal class CombinedViewModel : BaseViewModel
    {
        internal CombinedViewModel(ICache cache, HttpContext context)
        {
            Delimiter = Configuration.Delimiter;
            ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
            ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
            ExpandSingleBranches = Configuration.ExpandSingleBranches;

            AppPath = context.Request.ApplicationPath;
            Count = cache.Count;
            MemoryFree = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.
            ServerName = Environment.MachineName;

            //Will other shims have this?
            if (cache is HttpCacheShim)
            {
                var httpCache = cache as HttpCacheShim;
                MemoryLimitPercent = httpCache.EffectivePercentagePhysicalMemoryLimit;
                var memoryLimitKB = httpCache.EffectivePrivateBytesLimit;
                MemoryLimit = memoryLimitKB == -1 ? -1 : memoryLimitKB / 1024f;
            }

            Entries = cache.GetAll().Select(e => new Entry(e)).ToList();
        }

        public string Delimiter { get; set; }
        public bool ConfirmDeleteKey { get; set; }
        public bool ConfirmDeletePrefix { get; set; }
        public bool ExpandSingleBranches { get; set; }

        public string AppPath { get; set; }
        public int Count { get; set; }
        public float? MemoryFree { get; set; }
        public string ServerName { get; set; }

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

            //Timeout period? Can we get that?
            public string Key { get; set; }
            public string Type { get; set; }
        }
    }




    internal class CacheViewModel : BaseViewModel
    {
        internal CacheViewModel(IDictionary<string, object> entries)
        {
            Entries = entries.Select(e => new Entry(e)).ToList();
        }

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
            AppPath = context.Request.ApplicationPath;
            Count = cache.Count;
            MemoryFree = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.
            ServerName = Environment.MachineName;

            //Will other shims have this?
            if (cache is HttpCacheShim)
            {
                var httpCache = cache as HttpCacheShim;
                MemoryLimitPercent = httpCache.EffectivePercentagePhysicalMemoryLimit;
                var memoryLimitKB = httpCache.EffectivePrivateBytesLimit;
                MemoryLimit = memoryLimitKB == -1 ? -1 : memoryLimitKB / 1024f;
            }
        }

        public string AppPath { get; set; }
        public int Count { get; set; }
        public float? MemoryFree { get; set; }
        public string ServerName { get; set; }

        public float? MemoryLimit { get; set; }
        public long? MemoryLimitPercent { get; set; }
    }

    internal class SettingsViewModel : BaseViewModel
    {
        internal SettingsViewModel()
        {
            Delimiter = Configuration.Delimiter;
            ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
            ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
            ExpandSingleBranches = Configuration.ExpandSingleBranches;
        }

        public string Delimiter { get; set; }
        public bool ConfirmDeleteKey { get; set; }
        public bool ConfirmDeletePrefix { get; set; }
        public bool ExpandSingleBranches { get; set; }
    }
}
