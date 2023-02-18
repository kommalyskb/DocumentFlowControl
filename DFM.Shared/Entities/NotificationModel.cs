using DFM.Shared.DTOs;
using DFM.Shared.Helper;
using Minio.DataModel;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "NotificationModel" })]
    public class NotificationModel : HeaderModel
    {
        [Indexed]
        public string? Title { get; set; }
        [Indexed]
        public string? SendDate { get; set; } = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}";
        [Indexed]
        public string? SendFrom { get; set; }
        [Indexed]
        public ModuleType ModuleType { get; set; }
        [Indexed]
        public bool IsRead { get; set; }
        [Indexed]
        public string? ReadDate { get; set; }
        [Indexed(Sortable = true)]
        public decimal? TimeStamp { get; set; } = UniqueTxGenerator.NewTXDecimal();
        [Indexed]
        public string? RoleID { get; set; }
        [Indexed]
        public string? UserIDRead { get; set; }
        [Indexed]
        public string? DocID { get; set; }

    }
}
