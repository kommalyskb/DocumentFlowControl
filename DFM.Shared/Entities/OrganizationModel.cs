using DFM.Shared.Common;
using DFM.Shared.DTOs;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "OrganizationModel" })]
    public class OrganizationModel : HeaderModel
    {
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage Name { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public AttachmentModel Logo { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public List<RoleTreeModel> Chart { get; set; } = new();
    }
    public class RoleTreeModel
    {
        [Indexed]
        public RoleTypeModel RoleType { get; set; }
        [Indexed]
        public string? Publisher { get; set; }
        [Indexed(CascadeDepth = 1)]
        public PartialRole Role { get; set; } = new();
        [Indexed]
        public string? ParentID { get; set; }
        [Indexed(CascadeDepth = 1)]
        public PartialEmployeeProfile Employee { get; set; } = new();
        
        [Indexed]
        public bool CoProcess { get; set; } = false;
        [Indexed]
        public bool Follower { get; set; } = false;
       
    }
    
   
}
