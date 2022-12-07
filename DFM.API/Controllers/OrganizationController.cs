using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IEmployeeManager employeeManager;
        private readonly IOrganizationChart organizationChart;

        public OrganizationController(IEmployeeManager employeeManager, IOrganizationChart organizationChart)
        {
            this.employeeManager = employeeManager;
            this.organizationChart = organizationChart;
        }

        [HttpGet("GetRole")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<TabItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRoleV1(string? fakeId, CancellationToken cancellationToken = default(CancellationToken))
        {

            // Get UserID
            var userId = fakeId;

            if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
            {
                userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

            }

            // Get User Profile
            var userProfile = await employeeManager.GetProfile(userId, cancellationToken);

            if (!userProfile.Response.Success)
            {
                return BadRequest(userProfile.Response);
            }

            // Get role on organization
            var result = await organizationChart.GetRoles(userProfile.Content.OrganizationID!, userProfile.Content.id!, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Content);

            }
            return BadRequest(result.Response);
        }

        [HttpGet("GetItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetChartByID(id, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);

            }
            return BadRequest(result.Response);
        }
        [HttpGet("GetItem/{id}/{roleId}/{link}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, string roleId, string link, CancellationToken cancellationToken = default(CancellationToken))
        {
            ModuleType documentModule = ModuleType.DocumentInbound;
            if (link == "inbound")
            {
                documentModule = ModuleType.DocumentInbound;
            }
            else
            {
                documentModule = ModuleType.DocumentOutbound;
            }
            var result = await organizationChart.GetChartFrom(id, roleId, documentModule, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);

            }
            return BadRequest(result.Response);
        }


        [HttpGet("GetSupervisors/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSupervisorV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetSupervisorRolesPosition(orgId);
            if (result.Response.Success)
            {
                return Ok(result.Contents);
            }

            return BadRequest(result.Response);

        }

        [HttpGet("GetPublisher/{id}/{roleId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPublisherV1(string id, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetPublisher(id, roleId, cancellationToken);
            if (result.Success)
            {
                return Ok(result);

            }
            return BadRequest(result);
        }

        [HttpPost("NewItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1([FromBody] NewOrganizationRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }
        [HttpPost("AddRole/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRoleV1(string orgId, [FromBody] RoleTreeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }

        [HttpPut("UpdateRole/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRoleV1(string orgId, [FromBody] PartialRole request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }
        [HttpPut("UpdateEmployee/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmployeeV1(string orgId, [FromBody] PartialEmployeeProfile request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }

        [HttpGet("RemoveRole/{orgId}/{roleId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string orgId, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }
    }
}
