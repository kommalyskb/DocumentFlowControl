using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class ReportClickDTO
    {
        //TraceStatus status, string roleId
        public TraceStatus TraceStatus { get; set; }
        public string? RoleID { get; set; }
    }
}
