using DFM.Shared.Common;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RuleMenuController : ControllerBase
    {
        private readonly IRuleMenuManager menuManager;
        private readonly IEmployeeManager employeeManager;

        public RuleMenuController(IRuleMenuManager menuManager, IEmployeeManager employeeManager)
        {
            this.menuManager = menuManager;
            this.employeeManager = employeeManager;
        }

        [HttpGet("GetItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RuleMenu>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get Owner
            string userId = "";// GeneratorHelper.NotAvailable;
            if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
            {
                userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

            }
            var myProfile = await employeeManager.GetProfile(userId, cancellationToken);
            if (!myProfile.Response.Success)
            {
                return BadRequest(myProfile.Response);
            }

            var result = await menuManager.GetRuleMenus(userId, myProfile.Content.OrganizationID!, cancellationToken);

            return Ok(result);
        }

        [HttpPost("UpdateRule")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRuleV1([FromBody] RuleMenu request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get Owner
            string userId = "";// GeneratorHelper.NotAvailable;
            if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
            {
                userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

            }
            var myProfile = await employeeManager.GetProfile(userId, cancellationToken);
            if (!myProfile.Response.Success)
            {
                return BadRequest(myProfile.Response);
            }

            var result = await menuManager.UpdateRules(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);

            }
            return Ok(result);
        }
    }
}
