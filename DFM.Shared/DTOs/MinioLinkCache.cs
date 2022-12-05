using Minio.DataModel;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "MinioLinkCache" })]
    public class MinioLinkCache
    {
        [RedisIdField]
        public string? Id { get; set; }
        [Indexed]
        public string? Link { get; set; }
    }
}
