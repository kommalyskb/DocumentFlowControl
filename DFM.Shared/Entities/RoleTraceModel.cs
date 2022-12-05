using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class RoleTraceModel
    {
        [Indexed]
        public string? RoleID { get; set; }
        [Indexed]
        public RoleTypeModel RoleType { get; set; }
        [Indexed]
        public MultiLanguage Position { get; set; } = new();
        [Indexed]
        public ShortEmpInfo Fullname { get; set; } = new();
    }
}
