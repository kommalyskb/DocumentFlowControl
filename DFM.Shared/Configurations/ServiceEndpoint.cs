using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class ServiceEndpoint
    {
        public string? API { get; set; }
        public string? IdentityAPI { get; set; }
        public string? Frontend { get; set; }
        public string? PublicAPI { get; set; }
        public string? StorageAPI { get; set; }
        public string? MinioAPI { get; set; }
    }
}
