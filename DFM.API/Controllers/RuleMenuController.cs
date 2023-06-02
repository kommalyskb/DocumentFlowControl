using DFM.Shared.Common;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

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
        private readonly IOrganizationChart organizationChart;

        public RuleMenuController(IRuleMenuManager menuManager, IEmployeeManager employeeManager, IOrganizationChart organizationChart)
        {
            this.menuManager = menuManager;
            this.employeeManager = employeeManager;
            this.organizationChart = organizationChart;
        }

        [HttpGet("GetItem")]
        //[AllowAnonymous]
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
            var myRoles = await organizationChart.GetRoles(myProfile.Content.OrganizationID!, myProfile.Content.id!, cancellationToken);

            var result = await menuManager.GetRuleMenus(myRoles.Content.Select(x => x.Role.RoleType), myProfile.Content.OrganizationID!, cancellationToken);

            return Ok(result);
        }

        [HttpGet("GetRules/{orgId}")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RuleMenu>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRulesV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {

            var result = await menuManager.GetRuleMenus(orgId, cancellationToken);

            return Ok(result);
        }

        [HttpGet("RemoveItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {

            var result = await menuManager.RemoveRule(id, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
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

        [HttpGet("UpdateCache/{orgID}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCacheV1(string orgID, CancellationToken cancellationToken = default(CancellationToken))
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

            var result = await menuManager.UpdateCache(orgID, cancellationToken);
            
            return Ok($"Tasks: {result}");
        }
    }
}
