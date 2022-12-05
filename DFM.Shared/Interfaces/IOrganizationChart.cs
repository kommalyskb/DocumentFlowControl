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
    public interface IOrganizationChart
    {
        Task<CommonResponse> NewOrganization(MultiLanguage multiLanguage, AttachmentModel attachmentModel, CancellationToken cancellationToken = default);
        Task<CommonResponse> AddRoleAndEmployee(string id, RoleTreeModel roleTreeModel, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, OrganizationModel Content)> GetOrganization(string id, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveRoleAndEmployee(string id, string roleId, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveRoleAndEmployee(string id, List<string> roles, CancellationToken cancellationToken = default);
        Task<CommonResponse> AddRoleAndEmployee(string id, List<RoleTreeModel> roleTreeModel, CancellationToken cancellationToken = default);
        Task<CommonResponse> EditRoleOnly(string id, PartialRole roleTreeModel, CancellationToken cancellationToken = default);
        Task<CommonResponse> EditEmployeeOnly(string id, PartialEmployeeProfile employeeProfile, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<RoleTreeModel> Contents, CommonResponse Response)> GetChartFrom(string id, string roleId, ModuleType moduleType, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, IEnumerable<TabItemDto> Content)> GetRoles(string orgId, string userId, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<RoleTreeModel> Contents, CommonResponse Response)> GetChartByID(string id, CancellationToken cancellationToken = default);
        Task<CommonResponse> SaveDynamicFlow(string orgID, RoleTypeModel source, List<RoleTypeModel> targets, ModuleType moduleType, bool isCross = false, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, DynamicFlowModel FlowModel, IEnumerable<DynamicItem> Roles)> GetDynamicFlowByID(string id, ModuleType moduleType, CancellationToken cancellationToken = default);
        Task<bool> IsInSameParent(string? orgId, string? child1, string? child2, CancellationToken cancellationToken = default);
        Task<CommonResponseId> GetPublisher(string id, string roleId, CancellationToken cancellationToken = default);
    }
}
