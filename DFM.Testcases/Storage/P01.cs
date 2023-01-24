using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.Storage
{
    public class P01
    {
        IMinioService minioService;
        public P01()
        {
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                Server = "20.10.100.91",
                Port = 12000,
                Password = "1qaz2wsx"
            });
            minioService = new MinioService(new StorageConfiguration
            {
                AccessKey = "16493905036800000",
                SecretKey = "58b5e8134c3c4666b1dbb078561a6893",
                DefaultExpired = 8600,
                Endpoint = "20.10.100.92:9000",
                WithSSL = false,
                
            }, redisConnector);
        }

        [Fact]
        public async Task GenerateLink()
        {
            var result = await minioService.GenerateLink("test", "after.PNG");
        }
    }
}
