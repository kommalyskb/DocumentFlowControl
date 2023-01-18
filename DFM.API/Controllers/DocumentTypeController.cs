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
    public class DocumentTypeController : ControllerBase
    {
        private readonly IDocumentType documentType;

        public DocumentTypeController(IDocumentType documentType)
        {
            this.documentType = documentType;
        }

        [HttpGet("GetItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(DataTypeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentType.GetDocumentType(id, cancellationToken);
            if (!result.Response.Success)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Content);
        }

        [HttpGet("GetItems/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<DataTypeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentType.GetDocumentTypeByOrgId(orgId, cancellationToken);
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
        public async Task<IActionResult> NewItemV1([FromBody] DataTypeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await documentType.NewDocumentType(request, cancellationToken);
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
        public async Task<IActionResult> UpdateItemV1([FromBody] DataTypeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
                var result = await documentType.EditDocumentType(request, cancellationToken);
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
            var result = await documentType.RemoveDocumentType(id, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
