using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IDocumentSecurity
    {
        Task<CommonResponse> EditSecurityLevel(DocumentSecurityModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, DocumentSecurityModel Content)> GetSecurityLevel(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<DocumentSecurityModel> Contents, CommonResponse Response)> GetSecurityLevelByOrgId(string orgId, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewSecurityLevel(DocumentSecurityModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveSecurityLevel(string id, CancellationToken cancellationToken = default);
    }
}
