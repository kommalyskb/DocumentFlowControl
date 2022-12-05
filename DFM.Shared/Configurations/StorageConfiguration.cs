using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class StorageConfiguration
    {
        public string? Endpoint { get; set; }
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
        public string? AesKey { get; set; } = "SflmTvhXWiGqRHPz0Mkr3dXnXvt3SLT6qJTajqiHSrg";
        public bool WithSSL { get; set; } = false;
        public int DefaultExpired { get; set; } = 86400;
        public long MaxLength { get; set; } = 1024 * 1024 * 25; // Maximum file size 25 MB
    }
}
