using System;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tacklr.CacheManager
{
    //Move this someplace else?
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ViewType
    {
        None,
        Defer,//special case
        Tree,
        //List,
    }

    internal static class Configuration
    {
        private const string SectionName = "cachemanager";

        static Configuration()
        {
            CacheManagerConfig = (CacheManagerConfigSection)ConfigurationManager.GetSection(SectionName) ?? new CacheManagerConfigSection();//Do we need this?
        }

        internal static bool AllowRemoteAccess
        {
            get
            {
                return CacheManagerConfig.Security.AllowRemoteAccess;
            }
        }

        internal static string BaseUrl//BaseUrlPath
        {
            get
            {
                return CacheManagerConfig.BaseUrl;
            }
        }

        internal static bool ConfirmDeleteKey
        {
            get
            {
                return CacheManagerConfig.Settings.ConfirmDeleteKey;
            }
        }

        internal static bool ConfirmDeletePrefix
        {
            get
            {
                return CacheManagerConfig.Settings.ConfirmDeletePrefix;
            }
        }

        internal static string Delimiter
        {
            get
            {
                return CacheManagerConfig.Settings.Delimiter;
            }
        }

        internal static ViewType DetailView
        {
            get
            {
                return CacheManagerConfig.Settings.DetailView;
            }
        }

        internal static bool ExpandSingleBranches
        {
            get
            {
                return CacheManagerConfig.Settings.ExpandSingleBranches;
            }
        }

        private static CacheManagerConfigSection CacheManagerConfig { get; set; }
    }

    internal class CacheManagerConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("baseUrl", IsKey = false, IsRequired = true, DefaultValue = "/CacheManager.axd/")]
        internal string BaseUrl
        {
            get
            {
                var baseUrl = (string)base["baseUrl"];
                return baseUrl.TrimEnd('/') + "/";//Make sure we have the /
            }
            set
            {
                base["baseUrl"] = value;
            }
        }

        [ConfigurationProperty("security")]
        internal SecurityConfigElement Security
        {
            get
            {
                return (SecurityConfigElement)this["security"];
            }
            set
            {
                this["security"] = value;
            }
        }

        [ConfigurationProperty("settings")]
        internal SettingsConfigElement Settings
        {
            get
            {
                return (SettingsConfigElement)this["settings"];
            }
            set
            {
                this["settings"] = value;
            }
        }
    }

    internal class SecurityConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("allowRemoteAccess", IsKey = false, IsRequired = true, DefaultValue = false)]
        internal bool AllowRemoteAccess
        {
            get
            {
                return (bool)base["allowRemoteAccess"];
            }
            set
            {
                base["allowRemoteAccess"] = value;
            }
        }
    }

    internal class SettingsConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("confirmDeleteKey", IsKey = false, IsRequired = false, DefaultValue = true)]
        internal bool ConfirmDeleteKey
        {
            get
            {
                return (bool)base["confirmDeleteKey"];
            }
            set
            {
                base["confirmDeleteKey"] = value;
            }
        }

        [ConfigurationProperty("confirmDeletePrefix", IsKey = false, IsRequired = false, DefaultValue = true)]
        internal bool ConfirmDeletePrefix
        {
            get
            {
                return (bool)base["confirmDeletePrefix"];
            }
            set
            {
                base["confirmDeletePrefix"] = value;
            }
        }

        //Required value?
        [ConfigurationProperty("delimiter", IsKey = false, IsRequired = false, DefaultValue = "/")]
        internal string Delimiter
        {
            get
            {
                return (string)base["delimiter"];
            }
            set
            {
                base["delimiter"] = value;
            }
        }

        [ConfigurationProperty("detailView", IsKey = false, IsRequired = false, DefaultValue = "Defer")]
        internal ViewType DetailView
        {
            get
            {
                return (ViewType)base["detailView"];
            }
            set
            {
                base["detailView"] = value;//check value?
            }
        }

        [ConfigurationProperty("expandSingleBranches", IsKey = false, IsRequired = false, DefaultValue = false)]
        internal bool ExpandSingleBranches
        {
            get
            {
                return (bool)base["expandSingleBranches"];
            }
            set
            {
                base["expandSingleBranches"] = value;
            }
        }
    }
}