using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Extensions
{
    public static class ConfigExtensions
    {
        public static void ThrowIfNullOrEmpty(this string input, string reason, string name)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new InvalidOperationException(reason.Replace("{Name}", name));
        }
    }
}
