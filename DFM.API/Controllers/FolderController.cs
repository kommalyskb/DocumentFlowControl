using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCouch;
using MyCouch.Requests;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Threading;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class FolderController : ControllerBase
    {
        private readonly IFolderManager folderManager;

        public FolderController(IFolderManager folderManager)
        {
            this.folderManager = folderManager;
        }


        [HttpGet("GetItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(FolderModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await folderManager.GetFolder(id, cancellationToken);
            if (!result.Response.Success)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Content);
        }

        /// <summary>
        /// ດຶງເອົາ Folder ມາສະແດງ ຕາມ RoleID ຂອງຜູ້ທີ່ສາມາດແກ້ໄຂໄດ້
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="link"></param>
        /// <param name="view">ແມ່ນໃຊ້ໃນການສະແດງ Folder ທັງຫມົດ ຖ້າໃນກໍລະນີເປັນ 1</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("GetItems/{roleId}/{link}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<FolderModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string roleId, string link, int? view = 0, CancellationToken cancellationToken = default(CancellationToken))
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

        [HttpPost("NewItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1([FromBody] FolderModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await folderManager.NewFolder(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpPost("UpdateItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1([FromBody] FolderModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            var result = await folderManager.EditFolder(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("RemoveItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await folderManager.RemoveFolder(id, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
