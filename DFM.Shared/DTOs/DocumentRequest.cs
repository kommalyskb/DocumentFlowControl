using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class DocumentRequest
    {
        public DocumentModel? DocumentModel { get; set; } = new();
        public RawDocumentData? RawDocument { get; set; } = new();
        public List<RoleTreeModel>? CoProcesses { get; set; } = new();
        public MainReceiver? Main { get; set; } = new();
        public string? Uid { get; set; }
    }
}
