using System.Text;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace CloudflareSuperSites.Services.Minio
{
    public class MinioService : IMinioService
    {

        private IMinioClient CreateClient(string accountId, string accessKey, string secretKey)
        {
            return new MinioClient()
                .WithEndpoint($"{accountId}.r2.cloudflarestorage.com")
                .WithCredentials(accessKey, secretKey)
                .WithSSL(true)
                .WithRegion("auto")
                .Build();
        }

        public async Task UploadFile(string objectName, Stream value, string accountId, string accessKey, string secretKey, string bucketName, CancellationToken token)
        {
            var client = CreateClient(accountId, accessKey, secretKey);
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(value)
                .WithObjectSize(value.Length);
            await client.PutObjectAsync(putObjectArgs, token);
        }

        /// <summary>
        /// Note: This does load the file fully into Memory, only to be used by Manifest
        /// </summary>
        /// <param name="storageConfiguration"></param>
        /// <param name="objectName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string?> GetFile(string objectName, string accountId, string accessKey, string secretKey, string bucketName, CancellationToken token)
        {
            var memoryStream = new MemoryStream();
            var client = CreateClient(accountId, accessKey, secretKey);

            var listObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream((async (stream, cancellationToken) =>
                    await stream.CopyToAsync(memoryStream, cancellationToken)));
            try
            {
                var objectStat = await client.GetObjectAsync(listObjectArgs, token);
            }
            catch (ObjectNotFoundException) // Yum
            {
                return null;
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        public async Task<bool> DeleteFile(string objectName, string accountId, string accessKey, string secretKey, string bucketName, CancellationToken token)
        {
            var client = CreateClient(accountId, accessKey, secretKey);

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            try
            {
                await client.RemoveObjectAsync(removeObjectArgs, token);
            }
            catch (ObjectNotFoundException) // NOM NOM NOM NOM
            {
                
                return false;
            }

            return true;
        }




    }
}
