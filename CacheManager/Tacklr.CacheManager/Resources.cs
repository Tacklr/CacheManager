using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Tacklr.CacheManager
{
    internal static class Resources
    {
        private static IList<string> ResourceNames { get; set; }

        private static DateTime? s_BuildTime { get; set; }
        internal static DateTime BuildTime
        {
            get
            {
                if (!s_BuildTime.HasValue)
                    s_BuildTime = DateTime.Parse(Properties.Resources.BuildTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                return s_BuildTime.Value;
            }
        }

        private static string s_BundleToken { get; set; }
        internal static string BundleToken
        {
            get
            {
                if (String.IsNullOrEmpty(s_BundleToken))
                    s_BundleToken = Helpers.Sha1Hex(Encoding.UTF8.GetBytes(Properties.Resources.BuildTime));
                return s_BundleToken;
            }
        }

        private static string s_CssBundleToken { get; set; }
        internal static string CssBundleToken
        {
            get
            {
                if (String.IsNullOrEmpty(s_CssBundleToken))
                    s_CssBundleToken = Helpers.Sha1Hex(Resources.GetResourceBytes("combined.min.css"));
                return s_CssBundleToken;
            }
        }

        private static string s_JsBundleToken { get; set; }
        internal static string JsBundleToken
        {
            get
            {
                if (String.IsNullOrEmpty(s_JsBundleToken))
                    s_JsBundleToken = Helpers.Sha1Hex(Resources.GetResourceBytes("combined.min.js"));
                return s_JsBundleToken;
            }
        }

        static Resources()
        {
            ResourceNames = Startup.Assembly.GetManifestResourceNames().ToList();
        }

        internal static bool ResourceExists(string name)
        {
            return ResourceNames.Any(n => n.EndsWith(name));
        }

        internal static Stream GetResourceStream(string name)
        {
            var resource = ResourceNames.FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            if (String.IsNullOrEmpty(resource))
                throw new ArgumentException(String.Format("Embedded resource '{0}' not found.", name), "resource");
            return Startup.Assembly.GetManifestResourceStream(resource);
        }

        internal static string GetResourceString(string resource)
        {
            var stream = GetResourceStream(resource);
            if (stream == default(Stream))
                return default(string);

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        internal static byte[] GetResourceBytes(string resource)
        {
            var stream = GetResourceStream(resource);
            if (stream == default(Stream))
                return default(byte[]);

            using (var reader = new MemoryStream())
            {
                stream.CopyTo(reader);
                return reader.ToArray();
            }
        }
    }
}
