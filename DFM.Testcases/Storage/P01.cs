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
        IRedisConnector cache;
        public P01()
        {
            minioService = new MinioService(new StorageConfiguration
            {
                AccessKey = "LJ57QC66MZR7W1DAZTJP",
                SecretKey = "LIqUg44iaHY3zxDI9ErvVb0MxY7spvZ1amg2n3or",
                DefaultExpired = 8600,
                Endpoint = "storage-mtcv3.eoffice.la",
                WithSSL = true,
                
            }, cache);
        }

        [Fact]
        public async Task GenerateLink()
        {
            var result = await minioService.GenerateLink("test03", "eoffice-note.rar");
        }
    }
}
