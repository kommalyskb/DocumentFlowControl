using DFM.Shared.Common;
using DFM.Shared.Helper;
using Newtonsoft.Json;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public class Reciepient
    {
        [Indexed]
        public string? UId { get; set; } = Guid.NewGuid().ToString("N");
        [Indexed]
        public string? DataID { get; set; } // ແມ່ນ Unique ID ທີ່ເອົາໄວ້ Map ກັບຂໍ້ມູນເອກະສານ ທີ່ຢູ່ໃນ Field RawData
        [Indexed]
        public string? ReceiveDate { get; set; }
        [Indexed]
        public bool IsRead { get; set; }
        [Indexed]
        public string? ReadDate { get; set; }
        [Indexed]
        public TraceStatus DocStatus { get; set; }
        [Indexed]
        public TraceStatus BeforeMoveToTrash { get; set; }
        [Indexed]
        public OperationType SendRoleType { get; set; }
        [Indexed]
        public OperationType ReceiveRoleType { get; set; }
        [Indexed]
        public string? SendDate { get; set; }
        [Indexed(CascadeDepth = 1)]
        public CommentModel Comment { get; set; } = new();
        [Indexed]
        public string? ParentID { get; set; }
        [Indexed(CascadeDepth = 1)]
        public RoleTraceModel RecipientInfo { get; set; } = new();
        [Indexed(Sortable = true)]
        public decimal CreateDate { get; set; } = Convert.ToDecimal(DateTime.Now.ToString("yyyyMMddHHmmss"));
        [Indexed]
        public BehaviorStatus Behavior { get; set; }
        [Indexed]
        public InitialStatus InitialStatus { get; set; }
        [Indexed]
        public bool IsDisplay { get; set; } = true; // ແມ່ນ ໃຊ້ໃນກໍລະນີ ທີ່ບໍ່ຢາກສະແດງໃຫ້ Role ທີ່ຕິດພັນກັບເອກະສານ, ເຊີ່ງໂດຍມາດຕະຖານຈະເປັນຄ່າ True, ແຕ່ເມື່ອທີ່ມີການສົ່ງ ຈາກ Outbound ອອກມາ Inbound ຈະເປັນຄ່າ False ທັນທີ່ ເພື່ອປ້ອງກັນບໍ່ໃຫ້ Inbound ເບີ່ງເຫັນເອກະສານ
    }
    
}
