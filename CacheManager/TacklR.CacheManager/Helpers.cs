using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TacklR.CacheManager
{
    internal static class Helpers
    {
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
    }
}
