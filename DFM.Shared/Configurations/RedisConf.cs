using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class RedisConf
    {
        public string? Server { get; set; }
        public int? Port { get; set; }
        public string? Instance { get; set; }
        public int? Expire { get; set; }// Expire In second
        public string? User { get; set; }
        public string? Password { get; set; }
    }
}
