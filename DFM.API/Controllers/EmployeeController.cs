using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using HttpClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Xml.Linq;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeManager employeeManager;
        private readonly IIdentityHelper identityHelper;
        private readonly IHttpService httpService;
        private readonly ServiceEndpoint endpoint;

        public EmployeeController(IEmployeeManager employeeManager, IIdentityHelper identityHelper, IHttpService httpService, ServiceEndpoint endpoint)
        {
            this.employeeManager = employeeManager;
            this.identityHelper = identityHelper;
            this.httpService = httpService;
            this.endpoint = endpoint;
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
        public async Task<IActionResult> GetItemsV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await employeeManager.GetProfileByOrgId(orgId);
            if (result.RowCount == 0)
            {
                return BadRequest(result.Response);
            }
            return Ok(result.Contents);
        }

        [HttpPost("SaveItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveItemV1([FromBody] EmployeeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new CommonResponse
                {
                    Code = nameof(ResultCode.USERNAME_COULD_NOT_EMPTY),
                    Success = false,
                    Message = ResultCode.USERNAME_COULD_NOT_EMPTY,
                    Detail = ResultCode.USERNAME_COULD_NOT_EMPTY
                });
            }
            //get admin token
            var checkAdminToken = await identityHelper.GetAdminAccessToken();
            var validateResult = await identityHelper.ValidateUser(request.Username, checkAdminToken.Token);

            bool isNewUser = false;

            if (validateResult.SearchState == 2)
            {
                // Found user
                request.id = validateResult.UserID;
                isNewUser = false;
            }
            else
            {
                isNewUser = true;
                // Register new User
                var r = new UserRegisterRequestResponse
                {
                    userName = request.Username,
                    accessFailedCount = 0,
                    email = request.Contact.Email,
                    emailConfirmed = true,
                    lockoutEnabled = false,
                    lockoutEnd = DateTime.UtcNow,
                    phoneNumber = request.Contact.Phone,
                    phoneNumberConfirmed = true,
                    twoFactorEnabled = false

                };
                var reqUser = await httpService.Post<UserRegisterRequestResponse>($"{endpoint.IdentityAPI}/api/Users", r, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);
                if (reqUser.Success)
                {
                    var reqUserContent = await reqUser.HttpResponseMessage.Content.ReadAsStringAsync();
                    var userResponse = JsonSerializer.Deserialize<UserRegisterRequestResponse>(reqUserContent);
                    request.id = userResponse?.id;
                }
                else
                {
                    return BadRequest(new CommonResponse
                    {
                        Code = nameof(ResultCode.REG_USER_FAIL),
                        Success = false,
                        Message = ResultCode.REG_USER_FAIL,
                        Detail = ResultCode.REG_USER_FAIL
                    });
                }
            }

            if (isNewUser)
            {

                // New employee
                var result = await employeeManager.NewEmployeeProfile(request, cancellationToken);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            else
            {
                // Update
                var result = await employeeManager.EditEmployeeProfile(request, cancellationToken);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
        }


        [HttpGet("RemoveItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Ok();
        }
    }
}
