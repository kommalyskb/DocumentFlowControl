using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RoleManagerController : ControllerBase
    {
        private readonly IRoleManager roleManager;

        public RoleManagerController(IRoleManager roleManager)
        {
            this.roleManager = roleManager;
        }

        [HttpGet("GetItem/{orgId}/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(RoleManagementModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string orgId, string id)
        {
            return Ok();
        }

        [HttpGet("GetItems/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleManagementModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string orgId)
        {
            return Ok();
        }

        [HttpPost("GetItems")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleManagementModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1([FromBody] List<string> rolesId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await roleManager.GetRolesPosition(rolesId, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);
            }

            return BadRequest(result.Response);
            
        }

        [HttpPost("NewItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1(string orgId, [FromBody] RoleManagementModel request)
        {
            return Ok();
        }

        [HttpPut("UpdateItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1(string orgId, [FromBody] RoleManagementModel request)
        {
            return Ok();
        }

        [HttpGet("RemoveItem/{orgId}/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string orgId, string id)
        {
            return Ok();
        }
    }
}
