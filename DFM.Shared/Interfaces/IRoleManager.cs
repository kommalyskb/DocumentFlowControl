using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IRoleManager
    {
        Task<CommonResponse> EditRolePosition(RoleManagementModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, RoleManagementModel Content)> GetRolePosition(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<RoleManagementModel> Contents, CommonResponse Response)> GetRolePositionByOrgID(string orgId, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, List<RoleManagementModel> Contents)> GetRolesPosition(List<string> roles, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewRolePosition(RoleManagementModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveRolePosition(string id, CancellationToken cancellationToken = default);
    }
}
