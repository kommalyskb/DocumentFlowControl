using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class GetPersonalReportRequest
    {
        public InboxType inboxType { get; set; }
        public List<string>? roleIDs { get; set; } = new();
        public decimal start { get; set; }
        public decimal end { get; set; }
    }
}
