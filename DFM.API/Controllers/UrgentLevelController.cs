using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
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
    public class UrgentLevelController : ControllerBase
    {
        private readonly IDocumentUrgent documentUrgent;

        public UrgentLevelController(IDocumentUrgent documentUrgent)
        {
            this.documentUrgent = documentUrgent;
        }

        [HttpGet("GetItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(DocumentUrgentModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentUrgent.GetUrgentLevel(id, cancellationToken);
            if (!result.Response.Success)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Content);
        }

        [HttpGet("GetItems/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<DocumentUrgentModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentUrgent.GetUrgentLevelByOrgId(orgId, cancellationToken);
            if (result.RowCount == 0)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Contents);
        }

        [HttpPost("NewItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1([FromBody] DocumentUrgentModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentUrgent.NewUrgentLevel(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("UpdateItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1([FromBody] DocumentUrgentModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentUrgent.EditUrgentLevel(request, cancellationToken);
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
            var result = await documentUrgent.RemoveUrgentLevel(id, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
