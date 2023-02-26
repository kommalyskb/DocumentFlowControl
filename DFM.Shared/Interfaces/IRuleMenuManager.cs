using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface IRuleMenuManager
    {
        Task<IEnumerable<RuleMenu>> GetRuleMenus(string userId, string orgId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RuleMenu>> GetRuleMenus(string orgId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RuleMenu>> GetRuleMenus(IEnumerable<RoleTypeModel> roles, string orgId, CancellationToken cancellationToken = default);
        Task<CommonResponseId> RemoveRule(string id, CancellationToken cancellationToken = default);
        Task<CommonResponseId> UpdateRules(RuleMenu request, CancellationToken cancellationToken = default);
    }
}
