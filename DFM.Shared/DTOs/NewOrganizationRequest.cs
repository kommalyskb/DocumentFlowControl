using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class NewOrganizationRequest
    {
        public MultiLanguage Name { get; set; } = new();
        public AttachmentModel Attachment { get; set; } = new();
    }
}
