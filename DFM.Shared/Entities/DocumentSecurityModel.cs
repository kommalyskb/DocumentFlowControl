using DFM.Shared.DTOs;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DocumentSecurityModel" })]
    public class DocumentSecurityModel : HeaderModel
    {
        [Indexed]
        public string? Level { get; set; }
        [Indexed]
        public string? OrganizationID { get; set; }
        [Indexed(Sortable = true)]
        public int SortOrder { get; set; }
        [Indexed]
        public List<RoleTypeModel>? Authorized { get; set; } = new(); // ແມ່ນຕຳແໜ່ງໃດສາມາດເບີ່ງ File ເອກະສານໄດ້ແດ່

    }
}
