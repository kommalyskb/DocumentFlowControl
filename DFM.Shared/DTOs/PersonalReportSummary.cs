using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class PersonalReportSummary
    {
        public int Total { 
            get
            {
                return Draft + InProgress + Finished;
            }
        }
        public int Draft { get; set; }
        public int InProgress { get; set; }
        public int Finished { get; set; }
        public string? Position { get; set; }
        public string? FullName { get; set; }

        public string? RoleID { get; set; }
    }
    public class ReportPersonalGroup
    {
        public string? RoleID { get; set; }
        public int Count { get; set; }
    }
}
