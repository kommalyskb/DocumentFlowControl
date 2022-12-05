using DFM.Shared.Common;
using DFM.Shared.DTOs;
using Newtonsoft.Json;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "RoleManagementModel" })]
    public class RoleManagementModel : HeaderModel
    {
        [Indexed]
        public RoleTypeModel RoleType { get; set; }
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage Display { get; set; } = new();
        [Indexed]
        public string? OrganizationID { get; set; }
        
    }

    
}
