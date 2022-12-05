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
    }
}
