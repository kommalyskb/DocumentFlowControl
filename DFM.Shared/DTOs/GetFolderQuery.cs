using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class GetFolderQuery
    {
        public string roleId { get; internal set; }
        public InboxType inboxType { get; internal set; }
    }
}
