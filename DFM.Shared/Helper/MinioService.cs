using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.Exceptions;
using DFM.Shared.Resources;
using DFM.Shared.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using DFM.Shared.Extensions;
using DFM.Shared.Entities;
using Redis.OM;
using DFM.Shared.DTOs;

namespace DFM.Shared.Helper
{
    public interface IMinioService
    {
        Task<(bool IsSuccess, string Code, string Message, string BucketName)> CreateBucket(string bucketName);
        Task<(bool IsSuccess, string Response)> GenerateLink(string bucketName, string fileName, string endPoint = "", string accessKey = "", string secretKey = "");
        Task<(bool IsSuccess, byte[] ByteStream, string Response)> GetObject(string bucketName, string fileName, string endPoint = "", string accessKey = "", string secretKey = "");
        Task<(bool IsSuccess, string Code, string Message, string BucketName, string Filename, int Size)> PutObject(string bucketName, string fileName, byte[] content);
        Task<(bool IsSuccess, string Code, string Message, string BucketName, string Filename)> RemoveObject(string bucketName, string fileName);
    }
    public class MinioService : IMinioService
    {
        private readonly StorageConfiguration configuration;
        private readonly IRedisConnector redisConnector;
        MinioClient minioClient;
        public MinioService(StorageConfiguration configuration, IRedisConnector redisConnector)
        {
            this.configuration = configuration;
            this.redisConnector = redisConnector;
        }
        public async Task<(bool IsSuccess, string Code, string Message, string BucketName)> CreateBucket(string bucketName)
        {
            bucketName = bucketName.ToLower();
            // Validate
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return (default, nameof(ResultCode.BUCKET_EMPTY), ResultCode.BUCKET_EMPTY, bucketName);
            }

            // Init client
            if (configuration.WithSSL)
            {
                minioClient = new MinioClient()
                .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                   .WithSSL()
                   .Build();
            }
            else
            {
                minioClient = new MinioClient()
                    .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                    .Build();
            }
            BucketExistsArgs bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            bool found = await minioClient.BucketExistsAsync(bucketExistsArgs);
            if (found)
            {
                return (default, nameof(ResultCode.BUCKET_EXIST), ResultCode.BUCKET_EXIST, bucketName);
            }
            MakeBucketArgs makeBucketArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
            await minioClient.MakeBucketAsync(makeBucketArgs);

            return (true, nameof(ResultCode.BUCKET_CREATED), ResultCode.BUCKET_CREATED, bucketName);
        }

        public async Task<(bool IsSuccess,
            string Code,
            string Message,
            string BucketName,
            string Filename,
            int Size)> PutObject(string bucketName, string fileName, byte[] content)
        {
            bucketName = bucketName.ToLower();
            // Validate params
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return (default, nameof(ResultCode.BUCKET_EMPTY), ResultCode.BUCKET_EMPTY, bucketName, fileName, content.Length);
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return (default, nameof(ResultCode.FILENAME_EMPTY), ResultCode.FILENAME_EMPTY, bucketName, fileName, content.Length);
            }
            if (content.Length <= 0)
            {
                return (default, nameof(ResultCode.CONTENT_EMPTY), ResultCode.CONTENT_EMPTY, bucketName, fileName, content.Length);
            }

            // Init client
            if (configuration.WithSSL)
            {
                minioClient = new MinioClient()
                   .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                   .WithSSL()
                   .Build();
            }
            else
            {
                minioClient = new MinioClient()
                    .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                    .Build();
            }

            // Make a bucket on the server, if not already present.
            BucketExistsArgs bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            bool found = await minioClient.BucketExistsAsync(bucketExistsArgs);
            if (!found)
            {
                MakeBucketArgs makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await minioClient.MakeBucketAsync(makeBucketArgs);
            }

            using (MemoryStream contents = new MemoryStream(content))
            {
                string mimeType = MimeMapping.MimeUtility.GetMimeMapping(fileName);

                PutObjectArgs obj = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithStreamData(contents)
                    .WithObjectSize(content.Length)
                    .WithContentType(mimeType)
                    .WithObject(fileName);
                await minioClient.PutObjectAsync(obj);

            }

            return (true, nameof(ResultCode.OBJECT_UPLOADED), ResultCode.OBJECT_UPLOADED, bucketName, fileName, content.Length);
        }

        public async Task<(bool IsSuccess,
            string Code,
            string Message,
            string BucketName,
            string Filename)> RemoveObject(string bucketName, string fileName)
        {
            bucketName = bucketName.ToLower();
            // Validate params
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return (default, nameof(ResultCode.BUCKET_EMPTY), ResultCode.BUCKET_EMPTY, bucketName, fileName);
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return (default, nameof(ResultCode.FILENAME_EMPTY), ResultCode.FILENAME_EMPTY, bucketName, fileName);
            }

            // Init client
            if (configuration.WithSSL)
            {
                minioClient = new MinioClient()
                   .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                .WithSSL()
                   .Build();
            }
            else
            {
                minioClient = new MinioClient()
                    .WithEndpoint(configuration.Endpoint)
                .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                .WithRegion("us-east-1")
                    .Build();
            }

            //find a bucket on server, if there're not just return response
            BucketExistsArgs bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            bool found = await minioClient.BucketExistsAsync(bucketExistsArgs);
            if (!found)
            {
                return (false, nameof(ResultCode.BUCKET_NOT_EXIST), ResultCode.BUCKET_NOT_EXIST, bucketName, fileName);
            }
            RemoveObjectArgs obj = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName);

            await minioClient.RemoveObjectAsync(obj);

            return (true, nameof(ResultCode.OBJECT_REMOVED), ResultCode.OBJECT_REMOVED, bucketName, fileName);
        }


        public async Task<(bool IsSuccess, string Response)> GenerateLink(string bucketName, string fileName,
            string endPoint = "", string accessKey = "", string secretKey = "")
        {

            //Use redis cache for cache product
            string recordKey = $"{bucketName}.{fileName}";

            var provider = new RedisConnectionProvider(redisConnector.Connection);
            var context = provider.RedisCollection<MinioLinkCache>();


            var item = await context.FindByIdAsync(recordKey);

            if (item is null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(endPoint))
                    {
                        configuration.Endpoint = endPoint;
                    }
                    if (!string.IsNullOrWhiteSpace(accessKey))
                    {
                        configuration.AccessKey = accessKey;
                    }
                    if (!string.IsNullOrWhiteSpace(secretKey))
                    {
                        configuration.SecretKey = secretKey;
                    }
                    // Setup Minio
                    if (configuration.WithSSL)
                    {
                        minioClient = new MinioClient()
                        .WithEndpoint(configuration.Endpoint)
                        .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                        .WithRegion("us-east-1")
                        .WithSSL()
                           .Build();
                    }
                    else
                    {
                        minioClient = new MinioClient()
                        .WithEndpoint(configuration.Endpoint)
                        .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                        .WithRegion("us-east-1")
                            .Build();
                    }

                    // Check bucket
                    BucketExistsArgs bucketExistsArgs = new BucketExistsArgs()
                        .WithBucket(bucketName);
                    bool found = await minioClient.BucketExistsAsync(bucketExistsArgs);
                    if (!found)
                    {
                        return (default, $"{bucketName} not found");
                    }

                    // Check file
                    StatObjectArgs obj = new StatObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(fileName);
                    await minioClient.StatObjectAsync(obj);

                    PresignedGetObjectArgs getObj = new PresignedGetObjectArgs()
                        .WithObject(fileName)
                        .WithBucket(bucketName)
                        .WithExpiry(configuration.DefaultExpired);

                    string presignedUrl = await minioClient.PresignedGetObjectAsync(getObj);

                    await context.InsertAsync(new MinioLinkCache
                    {
                       Id = recordKey,
                       Link = presignedUrl
                    },TimeSpan.FromDays(1));

                    return (true, presignedUrl);
                }
                catch (MinioException e)
                {
                    return (default, e.Message);
                }

            }
            else
            {
                return (true, item.Link);
            }

        }

        // Get object
        public async Task<(bool IsSuccess, byte[] ByteStream, string Response)> GetObject(string bucketName, string fileName,
            string endPoint = "", string accessKey = "", string secretKey = "")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(endPoint))
                {
                    configuration.Endpoint = endPoint;
                }
                if (!string.IsNullOrWhiteSpace(accessKey))
                {
                    configuration.AccessKey = accessKey;
                }
                if (!string.IsNullOrWhiteSpace(secretKey))
                {
                    configuration.SecretKey = secretKey;
                }
                // Setup Minio
                if (configuration.WithSSL)
                {
                    minioClient = new MinioClient()
                    .WithEndpoint(configuration.Endpoint)
                    .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                    .WithRegion("us-east-1")
                    .WithSSL()
                       .Build();
                }
                else
                {
                    minioClient = new MinioClient()
                    .WithEndpoint(configuration.Endpoint)
                    .WithCredentials(configuration.AccessKey, configuration.SecretKey)
                    .WithRegion("us-east-1")
                        .Build();
                }

                // Check bucket
                BucketExistsArgs bucketExistsArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minioClient.BucketExistsAsync(bucketExistsArgs);
                if (!found)
                {
                    return (default, default, $"{bucketName} not found");
                }

                // Check file
                StatObjectArgs obj = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);
                await minioClient.StatObjectAsync(obj);

                // Get/Download file as stream

                byte[] byteStream = null;
                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                            .WithBucket(bucketName)
                                            .WithObject(fileName)
                                            .WithCallbackStream((stream) =>
                                            {
                                                byteStream = ReadStream(stream);
                                            });
                await minioClient.GetObjectAsync(getObjectArgs);

                if (byteStream.Length > 0)
                {
                    return (true, byteStream, default);
                }

                return (false, default, "File not found");

            }
            catch (MinioException e)
            {
                return (default, default, e.Message);
            }
        }
        private static byte[] ReadStream(Stream responseStream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
