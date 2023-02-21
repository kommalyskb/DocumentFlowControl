using DFM.Shared.Helper;
using Newtonsoft.Json;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    [Document(IdGenerationStrategyName = nameof(StaticIncrementStrategy))]
    public class HeaderModel
    {
        [JsonProperty("id")]
        [RedisIdField]
        public string? id { get; set; }
        [JsonProperty("_rev")]
        [Indexed]
        public string? rev { get; set; }

        [JsonProperty("recordDate")]
        [Indexed]
        public string? RecordDate { get; set; } = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
        [JsonProperty("lastModified")]
        [Indexed]
        public string? LastModified { get; set; } = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
    }
}
