using CloudflareWorkerBundler.Services.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Bundler
{
    public interface IFileBundler
    {
        Task<string> ProduceBundle(IRouter routerToUse, DirectoryInfo directoryToBundle, List<FileInfo> filesToBundle, string[] fileExtensionsToBundle, long bundleFileSizeLimit, string KVNamespace, long KVFileSizeLimit, string R2BucketName, string APIToken, string accountId);
    }
}
