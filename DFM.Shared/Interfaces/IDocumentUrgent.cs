using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IDocumentUrgent
    {
        Task<CommonResponse> EditUrgentLevel(DocumentUrgentModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, DocumentUrgentModel Content)> GetUrgentLevel(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<DocumentUrgentModel> Contents, CommonResponse Response)> GetUrgentLevelByOrgId(string orgId, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewUrgentLevel(DocumentUrgentModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveUrgentLevel(string id, CancellationToken cancellationToken = default);
    }
}
