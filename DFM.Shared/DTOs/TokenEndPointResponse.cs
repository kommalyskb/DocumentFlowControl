using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "TokenEndPointResponse" })]
    public class TokenEndPointResponse : CommonResponse
    {
        [Indexed]
        public string? AccessToken { get; set; }
        [Indexed]
        public int? Expire { get; set; }
        [Indexed]
        public string? RefreshToken { get; set; }
        [RedisIdField]
        public string? Username { get; set; }
        [Indexed]
        public string? Password { get; set; }
    }
}
