using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class PartialRole
    {
        [Indexed]
        public string? RoleID { get; set; }
        [Indexed]
        public RoleTypeModel RoleType { get; set; }
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage Display { get; set; } = new();
    }
}
