using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IEmployeeManager
    {
        Task<CommonResponse> EditEmployeeProfile(EmployeeModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, EmployeeModel Content)> GetProfile(string id, CancellationToken cancellationToken = default);
        Task<(decimal RowCount, IEnumerable<EmployeeModel> Contents, CommonResponse Response)> GetProfileByOrgId(string orgId, CancellationToken cancellationToken = default);
        Task<CommonResponse> NewEmployeeProfile(EmployeeModel request, CancellationToken cancellationToken = default);
        Task<CommonResponse> RemoveEmployeeProfile(string id, CancellationToken cancellationToken = default);
    }
}
