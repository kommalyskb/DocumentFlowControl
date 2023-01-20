using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class CommonResponse
    {
        [Indexed]
        public string? Code { get; set; } = GeneratorHelper.NotAvailable;
        [Indexed]
        public string? Message { get; set; } = GeneratorHelper.NotAvailable;
        [Indexed]
        public bool Success { get; set; }
        [Indexed]
        public string? Detail { get; set; } = GeneratorHelper.NotAvailable;
    }
    public class CommonResponseId : CommonResponse
    {
        public string? Id { get; set; } = GeneratorHelper.NotAvailable;
    }
}
