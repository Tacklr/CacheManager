using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacklR.CacheManager
{
    internal static class Configuration
    {
        private const string SectionName = "cachemanager";
        private static CacheManagerConfigSection CacheManagerConfig { get; set; }

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

        internal static string BaseUrl
        {
            get
            {
                return CacheManagerConfig.BaseUrl;
            }
        }

        internal static string Delimiter
        {
            get
            {
                return CacheManagerConfig.Settings.Delimiter;
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

        internal static bool ExpandSingleBranches
        {
            get
            {
                return CacheManagerConfig.Settings.ExpandSingleBranches;
            }
        }

        internal static ViewType DetailView
        {
            get
            {
                return CacheManagerConfig.Settings.DetailView;
            }
        }
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
    }

    //Move this someplace else?
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum ViewType
    {
        None,
        Defer,//special case
        Tree,
        //List,
    }
}
