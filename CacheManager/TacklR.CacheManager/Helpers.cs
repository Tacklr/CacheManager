using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacklR.CacheManager
{
    internal static class Helpers
    {
        private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static float? GetAvailableMemory()
        {
            try
            {
                using (var pc = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    return pc.NextValue();
                }
            }
            catch
            {
                return default(float?);
            }
        }

        private static NameValueCollection s_SecurityHeaders { get; set; }
        internal static NameValueCollection SecurityHeaders
        {
            get
            {
                if (s_SecurityHeaders == null)
                {
                    //What if the header is already set?
                    s_SecurityHeaders = new NameValueCollection {
                        { "X-Frame-Options", "SameOrigin" },
                        { "X-Content-Type-Options", "nosniff" },
                        { "X-XSS-Protection", "1; mode=block" }
                    };
                }
                return s_SecurityHeaders;
            }
        }

        //Can we modify the 'this' value?
        internal static void Override(this NameValueCollection collection, NameValueCollection overrides)
        {
            overrides.AllKeys.ToList().ForEach(k => collection.Remove(k));
            collection.Add(overrides);
        }

        internal static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        internal static long? ToUnixMilliseconds(this DateTime? time)//long?
        {
            if (!time.HasValue)
                return default(long?);
            return time.Value.ToUnixMilliseconds();
        }

        internal static long ToUnixMilliseconds(this DateTime time)
        {
            var difference = time - UnixEpochUtc;
            var timestamp = difference.TotalMilliseconds;
            return (long)timestamp;
        }

        internal static long? ToMilliseconds(this TimeSpan? timespan)
        {
            if (!timespan.HasValue)
                return default(long?);
            return (long)timespan.Value.TotalMilliseconds;
        }
    }
}
