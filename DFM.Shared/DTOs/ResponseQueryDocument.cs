using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class ResponseQueryDocument
    {
        public decimal RowCount { get; set; }
        public IEnumerable<DocumentModel>? Contents { get; set; }
        public CommonResponse Response { get; set; } = new();
    }
}
