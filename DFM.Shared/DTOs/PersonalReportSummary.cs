using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class PersonalReportSummary
    {
        public int Total { get; set; }
        public int Draft { get; set; }
        public int InProgress { get; set; }
        public int Finished { get; set; }
    }
}
