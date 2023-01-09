using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class QueryReportResponse
    {
        public string? id { get; set; }
        public decimal createDate { get; set; }
        public DocumentModel? content { get; set; } = new();
    }
}
