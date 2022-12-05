using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public enum TraceStatus
    {
        Draft, // Draft
        InProgress, // Inbox
        Completed, // Completed
        Revoked,
        Terminated,
        Following,
        CoProccess,
        Rejected,
        Trash,// Move to bin
        Remove// Permanent remove the document
    }
}
