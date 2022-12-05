using DFM.Shared.DTOs;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DocumentUrgentModel" })]
    public class DocumentUrgentModel : HeaderModel
    {
        [Indexed]
        public string? Level { get; set; }
        [Indexed]
        public string? OrganizationID { get; set; }
    }
}
