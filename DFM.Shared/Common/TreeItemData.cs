using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class TreeItemData
    {
        public string? OrganizationID { get; set; }
        public bool IsExpanded { get; set; }
        public RoleTreeModel? Content { get; set; } = new();
        public HashSet<TreeItemData>? TreeItems { get; set; } = new();
        public TreeItemData(RoleTreeModel content, bool isExpanded, string? orgId)
        {
            Content = content;
            IsExpanded = isExpanded;
            OrganizationID = orgId;
        }

    }
}
