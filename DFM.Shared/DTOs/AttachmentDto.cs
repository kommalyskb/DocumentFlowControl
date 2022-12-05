using DFM.Shared.Common;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class AttachmentDto
    {
        public IBrowserFile? File { get; set; }
        public AttachmentModel Info { get; set; } = new();
        public string? Link { get; set; }
    }
}
