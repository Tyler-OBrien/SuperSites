using CloudflareWorkerBundler.Models.Configuration.Storage;
using Minio.Exceptions;
using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Minio
{
    public interface IMinioService
    {
        public Task UploadFile(string objectName, Stream value, string accountId, string accessKey,
            string secretKey, string bucketName, CancellationToken token);

        /// <summary>
        /// Note: This does load the file fully into Memory, only to be used by Manifest
        /// </summary>
        /// <param name="storageConfiguration"></param>
        /// <param name="objectName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<string?> GetFile(string objectName, string accountId, string accessKey, string secretKey,
            string bucketName, CancellationToken token);

        public Task<bool> DeleteFile(string objectName, string accountId, string accessKey, string secretKey,
            string bucketName, CancellationToken token);

    }
}
