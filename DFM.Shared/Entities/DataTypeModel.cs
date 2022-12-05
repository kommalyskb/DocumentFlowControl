using DFM.Shared.DTOs;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DataTypeModel" })]
    public class DataTypeModel : HeaderModel
    {
        [Indexed]
        public string? DocType { get; set; }
        [Indexed]
        public string? OrganizationID { get; set; }
        [Indexed(Sortable = true)]
        public int SortOrder { get; set; }
    }
}
