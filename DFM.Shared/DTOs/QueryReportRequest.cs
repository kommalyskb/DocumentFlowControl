using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{

    public class QueryReportRequest
    {
        public InboxType inboxType { get; set; }
        public string? roleID { get; set; }

    }

}
