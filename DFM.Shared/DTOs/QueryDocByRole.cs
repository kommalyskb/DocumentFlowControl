using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class QueryDocByRole
    {
        public string? RoleId { get; set; }
        public TraceStatus DocStatus { get; set; }
        public InboxType InboxType { get; set; }
    }
}
