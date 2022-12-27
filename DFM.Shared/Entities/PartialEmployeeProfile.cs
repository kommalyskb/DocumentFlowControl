using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class PartialEmployeeProfile
    {
        [Indexed]
        public string? UserID { get; set; } 
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage Name { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage FamilyName { get; set; } = new();
        [Indexed]
        public Gender Gender { get; set; }
    }
}
