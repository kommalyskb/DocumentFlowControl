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
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "FolderModel" })]
    public class FolderModel : HeaderModel
    {
        [Indexed]
        public string? Title { get; set; }
        [Indexed]
        public int Start { get; set; } = 1;
        [Indexed]
        public int NextNumber { get; set; } = 1;
        [Indexed(Sortable = true)]
        public int Seq { get; set; }
        [Indexed]
        public string? Prefix { get; set; }
        [Indexed]
        public string? StartDate { get; set; }
        [Indexed]
        public string? ExpiredDate { get; set; }
        [Indexed]
        public string? ShortName { get; set; }
        [Indexed]
        public string? FormatType { get; set; }
        [Indexed]
        public List<string> SupportDocTypes { get; set; } = new();
        [Indexed]
        public List<string> Supervisors { get; set; } = new();
        [Indexed]
        public string? OrganizationID { get; set; }
        [Indexed]
        public InboxType InboxType { get; set; }
        [Indexed]
        public List<int>? DocNoUsed { get; set; } = new();
    }
}
