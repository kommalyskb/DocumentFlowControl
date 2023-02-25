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
        Task<CommonResponseId> UpdateRules(RuleMenu request, CancellationToken cancellationToken = default);
    }
}
