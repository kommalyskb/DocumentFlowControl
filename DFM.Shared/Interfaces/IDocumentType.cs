using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IDocumentType
    {
        Task<CommonResponse> EditDocumentType(DataTypeModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, DataTypeModel Content)> GetDocumentType(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<DataTypeModel> Contents, CommonResponse Response)> GetDocumentTypeByOrgId(string orgId, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewDocumentType(DataTypeModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveDocumentType(string id, CancellationToken cancellationToken = default);
    }
}
