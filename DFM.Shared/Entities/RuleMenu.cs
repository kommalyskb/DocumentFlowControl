using DFM.Shared.DTOs;
using Minio.DataModel;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "RuleMenu" })]
    public class RuleMenu : HeaderModel
    {
        [Indexed]
        public string? Group { get; set; }
        [Indexed]
        public MenuLink Menu { get; set; }
        [Indexed]
        public IEnumerable<string>? UserID { get; set; }
        [Indexed]
        public string? OrgID { get; set; }
    }

    public enum MenuLink
    {
        MAIN_PAGE,
        USER_INFO,
        IB_INBOX,
        IB_COPROCESS,
        IB_DRAFT,
        IB_COMOMPLETED,
        IB_TRASH,
        OB_INBOX,
        OB_COPROCESS,
        OB_DRAFT,
        OB_COMOMPLETED,
        OB_TRASH,
        MON_IB,
        MON_OB,
        IB_FOLDER,
        OB_FOLDER,
        ORG_CHART,
        USER_MGR,
        DOC_TYPE,
        SEC_LEVEL,
        URGENT_LEVEL,
        REPORT_IB,
        REPORT_OB
    }
}
