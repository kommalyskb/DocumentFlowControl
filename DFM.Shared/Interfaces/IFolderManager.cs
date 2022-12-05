using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IFolderManager
    {
        Task<CommonResponse> EditFolder(FolderModel request, CancellationToken cancellationToken = default);
        int FindNextFolderNum(FolderModel folder);
        Task<(CommonResponse Response, FolderModel Content)> GetFolder(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<FolderModel> Contents, CommonResponse Response)> GetFolderByRoleID(string roleId, InboxType inboxType, int? view = 0, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewFolder(FolderModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveFolder(string id, CancellationToken cancellationToken = default);
    }
}
