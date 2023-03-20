using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Services.Storage
{
    public interface IGenericStorage
    {
        /// <summary>
        /// Optional Header, placed at start of script
        /// </summary>
        public string Header => "";

        // Base Configuration
        public IStorageConfiguration Configuration { get; }

        // Write File to storage and return response
        public Task<StorageResponse> Write(IRouter router, string fileHash, byte[] value, string fileName);
        // Delete a file, used in cleanup
        public Task<bool> Delete(string objectName);

        /// <summary>
        /// Used to decide when to store a file. By default, uses Configured File Size Limit and AllowedFileExtensions. Can be overiden.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual bool StoreFile(FileInfo file)
        {
            if (Configuration.FileSizeLimit != 0 && file.Length > Configuration.FileSizeLimit) return false;
            if (Configuration.AllowedFileExtensions != null && Configuration.AllowedFileExtensions.Any(extension => file.Extension.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) == false) return false;
            return true;
        }
        // Return a list of all files
        public Task<List<string>> List();
    }
}
