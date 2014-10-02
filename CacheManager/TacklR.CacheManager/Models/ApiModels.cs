using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
        internal CombinedViewModel(int count, HttpContext context, IDictionary<string, object> entries)
        {
            Delimiter = Configuration.Delimiter;
            ConfirmDeleteKey = Configuration.ConfirmDeleteKey;
            ConfirmDeletePrefix = Configuration.ConfirmDeletePrefix;
            ExpandSingleBranches = Configuration.ExpandSingleBranches;

            AppPath = context.Request.ApplicationPath;
            Count = count;
            FreeMemory = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.
            ServerName = Environment.MachineName;

            Entries = entries.Select(e => new Entry(e)).ToList();
        }

        public string Delimiter { get; set; }
        public bool ConfirmDeleteKey { get; set; }
        public bool ConfirmDeletePrefix { get; set; }
        public bool ExpandSingleBranches { get; set; }

        public string AppPath { get; set; }
        public int Count { get; set; }
        public float? FreeMemory { get; set; }
        public string ServerName { get; set; }

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
        internal StatsViewModel(int count, HttpContext context)
        {
            AppPath = context.Request.ApplicationPath;
            Count = count;
            FreeMemory = Helpers.GetAvailableMemory();//this was in the AspAlliance version, not sure if it's really any help to report.
            ServerName = Environment.MachineName;
        }

        public string AppPath { get; set; }
        public int Count { get; set; }
        public float? FreeMemory { get; set; }
        public string ServerName { get; set; }
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
