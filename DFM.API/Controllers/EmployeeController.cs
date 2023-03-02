using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Resources;
using HttpClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCouch.Requests;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Xml.Linq;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeManager employeeManager;
        private readonly IIdentityHelper identityHelper;
        private readonly IHttpService httpService;
        private readonly ServiceEndpoint endpoint;
        private readonly IAESHelper aes;
        private readonly AESConfig aesConf;
        private readonly IEmailHelper emailHelper;
        private readonly SMTPConf smtp;

        public EmployeeController(IEmployeeManager employeeManager, IIdentityHelper identityHelper, IHttpService httpService,
            ServiceEndpoint endpoint, IAESHelper aes, AESConfig aesConf, IEmailHelper emailHelper, SMTPConf smtp)
        {
            this.employeeManager = employeeManager;
            this.identityHelper = identityHelper;
            this.httpService = httpService;
            this.endpoint = endpoint;
            this.aes = aes;
            this.aesConf = aesConf;
            this.emailHelper = emailHelper;
            this.smtp = smtp;
        }

        [HttpGet("GetItem")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(EmployeeModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(CancellationToken cancellationToken = default(CancellationToken))
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
        public async Task<IActionResult> SaveItemV1([FromBody] EmployeeModel request, string notify, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
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
                if (string.IsNullOrWhiteSpace(request.id))
                {
                    // New User
                    isNewUser = true;
                }
                if (validateResult.SearchState == 2)
                {
                    // Found user
                    request.id = validateResult.UserID;

                    // Reset password
                    if (isNewUser)
                    {
                        string password = $"{request.Password}@Dfm.codecamp";

                        var reqUserContent = new UserResetRequest
                        {
                            userId = request.id,
                            password = password,
                            confirmPassword = password

                        };
                        Log.Information($"Reset User(New) SSO: {reqUserContent}");
                        // Set password
                        await httpService.Post<UserResetRequest>($"{endpoint.IdentityAPI}/api/Users/ChangePassword", reqUserContent, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);

                        // Send email
                        if (!string.IsNullOrWhiteSpace(notify))
                        {
                            if (notify == "yes")
                            {
                                try
                                {
                                    string emailBody = emailHelper.RegisterMailBody($"{request.Name.Local} {request.FamilyName.Local}", request.Username, request.Password!);
                                    await emailHelper.Send(new EmailProperty
                                    {
                                        Body = emailBody,
                                        From = smtp.Email,
                                        To = new List<string> { request.Contact.Email! },
                                        Subject = $"ລົງທະບຽນນຳໃຊ້ລະບົບຈໍລະຈອນເອກະສານ"
                                    });
                                }
                                catch (Exception)
                                {

                                }

                            }
                        }

                        // Encrypt password before save to database
                        if (aesConf.Base == BaseConfig.HEX)
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromHEX(), aesConf.IV!.FromHEX()).ToHEX();
                        }
                        else
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromBase64(), aesConf.IV!.FromBase64()).ToBase64();
                        }
                    }
                }
                else
                {
                    // Register new User
                    var r = new UserRegisterRequestResponse
                    {
                        userName = request.Username,
                        accessFailedCount = 0,
                        email = $"{request.Username}@dummy.csenergy.la",
                        emailConfirmed = true,
                        lockoutEnabled = true,
                        lockoutEnd = DateTime.UtcNow,
                        //phoneNumber = request.Contact.Phone,
                        //phoneNumberConfirmed = true,
                        twoFactorEnabled = false

                    };
                    var reqUser = await httpService.Post<UserRegisterRequestResponse>($"{endpoint.IdentityAPI}/api/Users", r, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);
                    var reqUserContent = await reqUser.HttpResponseMessage.Content.ReadAsStringAsync();
                    Log.Information($"Create User SSO: {reqUserContent}");
                    if (reqUser.Success)
                    {
                        var userResponse = JsonSerializer.Deserialize<UserRegisterRequestResponse>(reqUserContent);
                        request.id = userResponse?.id;
                        request.UserID = userResponse?.id;
                        string password = $"{request.Password}@Dfm.codecamp";
                        // Send email
                        if (!string.IsNullOrWhiteSpace(notify))
                        {
                            if (notify == "yes")
                            {
                                try
                                {
                                    string emailBody = emailHelper.RegisterMailBody($"{request.Name.Local} {request.FamilyName.Local}", request.Username, request.Password!);
                                    await emailHelper.Send(new EmailProperty
                                    {
                                        Body = emailBody,
                                        From = smtp.Email,
                                        To = new List<string> { request.Contact.Email! },
                                        Subject = $"ລົງທະບຽນນຳໃຊ້ລະບົບຈໍລະຈອນເອກະສານ"
                                    });
                                }
                                catch (Exception)
                                {

                                }

                            }
                        }


                        // Set password
                        await httpService.Post<UserResetRequest>($"{endpoint.IdentityAPI}/api/Users/ChangePassword", new UserResetRequest
                        {
                            userId = userResponse?.id,
                            password = password,
                            confirmPassword = password

                        }, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);

                        if (aesConf.Base == BaseConfig.HEX)
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromHEX(), aesConf.IV!.FromHEX()).ToHEX();
                        }
                        else
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromBase64(), aesConf.IV!.FromBase64()).ToBase64();
                        }
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

                var existingFromDB = await employeeManager.GetProfile(request.id!);
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
                    if (existingFromDB.Response.Success)
                    {
                        // Update
                        var result = await employeeManager.EditEmployeeProfile(request, cancellationToken);
                        if (result.Success)
                        {
                            return Ok(result);
                        }
                        return BadRequest(result);
                    }
                    else
                    {
                        // This user is already delete from database but not delete from SSO
                        // Then try to reset password
                        // Set password
                        string password = $"{request.Password}@Dfm.codecamp";
                        // Send email
                        if (!string.IsNullOrWhiteSpace(notify))
                        {
                            if (notify == "yes")
                            {
                                try
                                {
                                    string emailBody = emailHelper.RegisterMailBody($"{request.Name.Local} {request.FamilyName.Local}", request.Username, request.Password!);
                                    await emailHelper.Send(new EmailProperty
                                    {
                                        Body = emailBody,
                                        From = smtp.Email,
                                        To = new List<string> { request.Contact.Email! },
                                        Subject = $"ລົງທະບຽນນຳໃຊ້ລະບົບຈໍລະຈອນເອກະສານ"
                                    });
                                }
                                catch (Exception)
                                {

                                }

                            }
                        }


                        await httpService.Post<UserResetRequest>($"{endpoint.IdentityAPI}/api/Users/ChangePassword", new UserResetRequest
                        {
                            userId = validateResult.UserID,
                            password = password,
                            confirmPassword = password

                        }, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);

                        if (aesConf.Base == BaseConfig.HEX)
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromHEX(), aesConf.IV!.FromHEX()).ToHEX();
                        }
                        else
                        {
                            request.Password = aes.Encrypt(password, aesConf.Key!.FromBase64(), aesConf.IV!.FromBase64()).ToBase64();
                        }
                        request.UserID = validateResult.UserID;
                        // New employee
                        var result = await employeeManager.NewEmployeeProfile(request, cancellationToken);

                        if (result.Success)
                        {
                            return Ok(result);
                        }
                        return BadRequest(result);
                    }
                    
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        [HttpPost("UpdateImageProfile/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImageProfileV1(string id, [FromBody] AttachmentModel image, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new CommonResponse
                    {
                        Code = nameof(ResultCode.EMPTY_ID),
                        Success = false,
                        Message = ResultCode.EMPTY_ID,
                        Detail = ResultCode.EMPTY_ID
                    });
                }

                var existingFromDB = await employeeManager.GetProfile(id, cancellationToken);
                if (!existingFromDB.Response.Success)
                {
                    return BadRequest(new CommonResponse
                    {
                        Code = nameof(ResultCode.NOT_FOUND),
                        Success = false,
                        Message = ResultCode.NOT_FOUND,
                        Detail = ResultCode.NOT_FOUND
                    });
                }

                // Set profile image
                existingFromDB.Content.ProfileImage = image;

                var result = await employeeManager.EditEmployeeProfile(existingFromDB.Content, cancellationToken);

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

        [HttpPost("ResetPassword")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPasswordV1([FromBody] EmployeeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
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

                string password = $"{request.Password}@Dfm.codecamp";

                // Set password
                await httpService.Post<UserResetRequest>($"{endpoint.IdentityAPI}/api/Users/ChangePassword", new UserResetRequest
                {
                    userId = request.id,
                    password = password,
                    confirmPassword = password

                }, new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);

                if (aesConf.Base == BaseConfig.HEX)
                {
                    request.Password = aes.Encrypt(password, aesConf.Key!.FromHEX(), aesConf.IV!.FromHEX()).ToHEX();
                }
                else
                {
                    request.Password = aes.Encrypt(password, aesConf.Key!.FromBase64(), aesConf.IV!.FromBase64()).ToBase64();
                }

                return Ok(new CommonResponseId
                {
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Success = true,
                    Id = request.Password,
                    Detail = ResultCode.SUCCESS_OPERATION,
                    Message = ResultCode.SUCCESS_OPERATION
                });
            }
            catch (Exception)
            {

                throw;
            }
           



        }

        [HttpPost("ResendRegisterEmail")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendRegisterEmailV1([FromBody] ResendRegisterEmail request, CancellationToken cancellationToken = default(CancellationToken))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string emailBody = emailHelper.RegisterMailBody($"{request.Name} {request.Surname}", request.Username!, request.Password!);
            await emailHelper.Send(new EmailProperty
            {
                Body = emailBody,
                From = smtp.Email,
                To = new List<string> { request.Email! },
                Subject = $"ລົງທະບຽນນຳໃຊ້ລະບົບຈໍລະຈອນເອກະສານ"
            });

            stopwatch.Stop();
            return Ok($"Elapsed: {stopwatch.Elapsed.TotalSeconds}");

        }


        [HttpGet("RemoveItem/{id}")]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await employeeManager.RemoveEmployeeProfile(id, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(result);
                }
                // Remove user from SSO
                //get admin token
                var checkAdminToken = await identityHelper.GetAdminAccessToken();
                var delUser = await httpService.Delete<object>($"{endpoint.IdentityAPI}/api/Users/{id}", new AuthorizeHeader("bearer", checkAdminToken.Token), cancellationToken);
                var delUserContent = await delUser.HttpResponseMessage.Content.ReadAsStringAsync();
                Log.Information($"Delete_User: {id}");

                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
