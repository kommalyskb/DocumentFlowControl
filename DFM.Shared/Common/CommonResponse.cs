using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class CommonResponse
    {
        public string? Code { get; set; } = GeneratorHelper.NotAvailable;
        public string? Message { get; set; } = GeneratorHelper.NotAvailable;
        public bool Success { get; set; }
        public string? Detail { get; set; } = GeneratorHelper.NotAvailable;
    }
    public class CommonResponseId : CommonResponse
    {
        public string? Id { get; set; } = GeneratorHelper.NotAvailable;
    }
}
