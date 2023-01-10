using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IDocumentTransaction
    {
        Task<CommonResponseId> EditDocument(DocumentModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, DocumentModel Content)> GetDocument(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<DocumentModel> Contents, CommonResponse Response)> GetDocumentByRoleId(string roleId, InboxType inboxType, TraceStatus traceStatus, CancellationToken cancellationToken = default);
        Task<List<PersonalReportSummary>> GetPersonalReport(GetPersonalReportRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentModel>> DrillDownReport(GetPersonalReportRequest request, TraceStatus docStatus, CancellationToken cancellationToken = default);
        Task<CommonResponseId> NewDocument(DocumentModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> SendDocument(string docId, List<Reciepient> reciepients, CancellationToken cancellationToken = default);
    }
}
