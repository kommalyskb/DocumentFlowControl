using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class DocumentDto
    {
        public string? Id { get; set; }
        public string? DocNo { get; set; }
        public string? Title { get; set; }
        public string? FormType { get; set; }
        public string? DocDate { get; set; }
        public string? UrgentLevel { get; set; }
        public bool IsRead { get; set; }
        public string? Uid { get; set; }
        public decimal CreateDate { get; set; }
        public string? FontColor { get; set; } = "color:black";
        public decimal CompletedDate { get; set; }
    }
}
