using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TacklR.CacheManager
{
    internal static class Resources
    {
        private static IList<string> ResourceNames { get; set; }
        private static Assembly Assembly { get; set; }

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

        static Resources()
        {
            Assembly = CacheManagerViewFactory.Assembly;//Can this be passed around like this?
            ResourceNames = Assembly.GetManifestResourceNames().ToList();
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
            return CacheManagerViewFactory.Assembly.GetManifestResourceStream(resource);
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
