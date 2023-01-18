using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class DashboardTableDto
    {
        public int InProgress { get; set; }
        public int Draft { get; set; }
        public int Completed { get; set; }
        public int Trash { get; set; }
        public int Total { get; set; }
    }
}
