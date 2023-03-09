using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class TabItemDto
    {
        public string? OrgID { get; set; }
        public PartialRole Role { get; set; } = new();
        public PartialEmployeeProfile Employee { get; set; } = new();
        public string? ParentID { get; set; }
    }
}
