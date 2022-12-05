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
    public class FolderController : ControllerBase
    {
        private readonly IFolderManager folderManager;

        public FolderController(IFolderManager folderManager)
        {
            this.folderManager = folderManager;
        }


        [HttpGet("GetItem/{orgId}/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(FolderModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string orgId, string id)
        {
            return Ok();
        }

        [HttpGet("GetItems/{roleId}/{link}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<FolderModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string roleId, string link, int? view, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Verify link
            InboxType inboxType = InboxType.Inbound;
            if (link == "inbound")
            {
                inboxType = InboxType.Inbound;
            }
            else if (link == "outbound")
            {
                inboxType = InboxType.Outbound;

            }
            else
            {
                return BadRequest(new CommonResponse
                {
                    Code = nameof(ResultCode.INVALID_LINK),
                    Message = ResultCode.INVALID_LINK,
                    Detail = ResultCode.INVALID_LINK,
                    Success = false
                });

            }
            var result = await folderManager.GetFolderByRoleID(roleId, inboxType, view, cancellationToken);
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
        public async Task<IActionResult> NewItemV1(string orgId, [FromBody] FolderModel request)
        {
            return Ok();
        }

        [HttpPut("UpdateItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1(string orgId, [FromBody] FolderModel request)
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
