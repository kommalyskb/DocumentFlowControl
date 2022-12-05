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
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeManager employeeManager;

        public EmployeeController(IEmployeeManager employeeManager)
        {
            this.employeeManager = employeeManager;
        }

        [HttpGet("GetItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string? fakeId, CancellationToken cancellationToken = default(CancellationToken))
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
            return Ok(userProfile.Content);
        }

        [HttpGet("GetItems/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string orgId)
        {
            var result = await employeeManager.GetProfileByOrgId(orgId);
            if (result.RowCount == 0)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Contents);
        }

        [HttpPost("NewItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1(string orgId, [FromBody] EmployeeModel request)
        {
            return Ok();
        }

        [HttpPut("UpdateItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1(string orgId, [FromBody] EmployeeModel request)
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
