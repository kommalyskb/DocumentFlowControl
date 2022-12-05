using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Helper;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DocumentModel" })]
    public class DocumentModel : HeaderModel
    {
        [Indexed]
        public string? OrganizationID { get; set; }
        [Indexed]
        public InboxType InboxType { get; set; }
        [Indexed(CascadeDepth = 1)]
        public List<Reciepient>? Recipients { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public List<RawDocumentData>? RawDatas { get; set; } = new();
        [Indexed]
        public string? ParentID { get; set; } // ແມ່ນ DocumentModel ID ທີ່ເປັນ Parent, ເຊີ່ງຈະມີກໍ່ຕໍ່ເມື່ອ ເປັນການສົ່ງ ຈາກ Outbound ມາຫາ Inbound ເພື່ອໃຊ້ໃນການ Track ເອກະສານ
    }
    
}
