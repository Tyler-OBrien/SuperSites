using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.Configuration;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Models.Middleware
{
    public interface IBaseMiddleware
    {
        public string Name { get; }

        public ExecutionOrder Order { get; }

        public void AddCode(IRouter router, StringBuilder stringBuilder, string fileName, string contentType, string fileHash, StorageResponse storageResponse, List<string> headers);



    }

    public enum ExecutionOrder
    {
        /// <summary>
        /// Modify Response Headers, at very start of flow..
        /// </summary>
        Headers,
        /// <summary>
        /// Execute at the very start, used for handling code that must change response / etc
        /// </summary>
        Start,
        /// <summary>
        /// Execute normal order
        /// </summary>
        Normal,
        /// <summary>
        /// Execute at end
        /// </summary>
        End,
    }
}
