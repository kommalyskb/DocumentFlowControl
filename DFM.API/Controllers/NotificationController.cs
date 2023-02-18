using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using DFM.Shared.Resources;
using HttpClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCouch.Requests;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationManager notificationManager;
        private readonly IEmployeeManager employeeManager;
        private readonly IOrganizationChart organizationChart;

        public NotificationController(INotificationManager notificationManager, IEmployeeManager employeeManager, IOrganizationChart organizationChart)
        {
            this.notificationManager = notificationManager;
            this.employeeManager = employeeManager;
            this.organizationChart = organizationChart;
        }

        [HttpGet("GetMyNotice")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<NotificationModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyNoticesV1(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Get UserID
                var userId = "";

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
                var orgs = await organizationChart.GetRoles(userProfile.Content.OrganizationID!, userProfile.Content.id!, cancellationToken);
                if (!orgs.Response.Success)
                {
                    return NotFound(orgs.Response);

                }

                //var isInRole = orgs.Content.Any(x => x.Role.RoleID == roleID);
                //if (!isInRole)
                //{
                //    return BadRequest(new CommonResponse
                //    {
                //        Code = nameof(ResultCode.AUTH_FAILED),
                //        Success = false,
                //        Detail = ResultCode.AUTH_FAILED,
                //        Message = ResultCode.AUTH_FAILED
                //    });
                //}
                var result = await notificationManager.ListNotices(orgs.Content.Select(x => x.Role.RoleID)!, cancellationToken);

                if (!result.Response.Success)
                {
                    return NotFound(result.Response);
                }

                return Ok(result.Contents);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost("Create/{docID}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateV1(string docID, [FromBody]NotificationModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Get UserID
                //var userId = "";

                //if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
                //{
                //    userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

                //}

                //// Get User Profile
                //var userProfile = await employeeManager.GetProfile(userId, cancellationToken);

                //if (!userProfile.Response.Success)
                //{
                //    return BadRequest(userProfile.Response);
                //}

                request.UserIDRead = "";
                request.IsRead = false;
                request.DocumentID = docID;
                
                var result = await notificationManager.CreateNotice(request, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("Read/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReadV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                // Get UserID
                var userId = "";

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

                var result = await notificationManager.ReadNotice(id, userId, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
