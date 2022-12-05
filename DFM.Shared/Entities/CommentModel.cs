using DFM.Shared.Common;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class CommentModel
    {
        [Indexed]
        public string? CommentDate { get; set; }
        [Indexed]
        public string? Comment { get; set; }
        [Indexed(CascadeDepth = 1)]
        public RoleTraceModel RoleTrace { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public List<AttachmentModel> Attachments { get; set; } = new();
    }
}
