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
    public class DocumentTypeController : ControllerBase
    {
        private readonly IDocumentType documentType;

        public DocumentTypeController(IDocumentType documentType)
        {
            this.documentType = documentType;
        }

        [HttpGet("GetItem/{orgId}/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(DataTypeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string orgId, string id)
        {
            return Ok();
        }

        [HttpGet("GetItems/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<DataTypeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemsV1(string orgId)
        {
            var result = await documentType.GetDocumentTypeByOrgId(orgId);
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
        public async Task<IActionResult> NewItemV1(string orgId, [FromBody] DataTypeModel request)
        {
            return Ok();
        }

        [HttpPut("UpdateItem/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateItemV1(string orgId, [FromBody] DataTypeModel request)
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
