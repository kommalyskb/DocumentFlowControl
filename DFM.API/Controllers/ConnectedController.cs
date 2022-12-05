using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Extensions;
using DFM.Shared.Resources;
using HttpClientService;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ConnectedController : ControllerBase
    {
        private readonly IHttpService httpService;
        private readonly IDistributedCache cache;
        private readonly OpenIDConf openId;
        private readonly RedisConf redis;

        public ConnectedController(IHttpService httpService, IDistributedCache cache, OpenIDConf openId, RedisConf redis)
        {
            this.httpService = httpService;
            this.cache = cache;
            this.openId = openId;
            this.redis = redis;
        }

        /// <summary>
        /// v1.0.0
        /// ແມ່ນ API ທີ່ໃຊ້ໃນການ ຂໍ access token ສຳລັບນັກພັດທະນາ ທີ່ບໍ່ສາມາດໃຊ້ oauth2.0
        /// ສຳລັບ flow ທີ່ support ຕອນນີ້ມີຢູ່ 2 ຢ່າງຄື: 
        /// 1. GrantType = "client_credentials" ສຳລັບ flow ນີ້ຈະບໍ່ support refresh token ແລະ ບໍ່ຮອງຮັບ offline access scope
        /// 2. GrantType = "password" ສຳລັບ flow ນີ້ແມ່ນ ຮອງຮັບ refresh token ແລະ ຮອງຮັບ offline access scope
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("token")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TokenEndPointResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TokenV1([FromBody] TokenEndPointRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            //start stopwatch
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Check cache
            string recordKey = $"connect.token.{request.ClientID}.{request.Username}"; // Set key for cache
            var cacheResult = await cache.GetRecordAsync<TokenEndPointResponse>(recordKey);

            if (cacheResult is null)
            {
                if (request.GrantType == "password")
                {
                    var response = await httpService.Client.RequestPasswordTokenAsync(new PasswordTokenRequest
                    {
                        Address = $"{openId.Authority}/connect/token",
                        GrantType = "password",

                        ClientId = request.ClientID,
                        ClientSecret = request.Secret,

                        UserName = request.Username,
                        Password = request.Password,

                        Scope = request.Scope

                    }, cancellationToken);

                    if (response.IsError)
                    {
                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return BadRequest(new CommonResponse
                        {
                            Success = false,
                            Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                            Message = response.Error
                        });
                    }

                    var model = new TokenEndPointResponse
                    {
                        Success = true,
                        AccessToken = response.AccessToken,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION,
                        Expire = response.ExpiresIn,
                        RefreshToken = response.RefreshToken,
                        Password = request.Password,
                        Username = request.Username
                    };

                    // Set cache
                    await cache.SetRecordAsync<TokenEndPointResponse>(recordKey, model, TimeSpan.FromSeconds(3600));
                    return Ok(model);
                }
                else if (request.GrantType == "client_credentials")
                {
                    var response = await httpService.Client.RequestTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = $"{openId.Authority}/connect/token",
                        GrantType = "client_credentials",

                        ClientId = request.ClientID,
                        ClientSecret = request.Secret,

                        Scope = request.Scope

                    }, cancellationToken);
                    if (response.IsError)
                    {
                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return BadRequest(new CommonResponse
                        {
                            Success = false,
                            Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                            Message = response.Error
                        });
                    }

                    var model = new TokenEndPointResponse
                    {
                        Success = true,
                        AccessToken = response.AccessToken,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION,
                        Expire = response.ExpiresIn,
                        RefreshToken = response.RefreshToken
                    };

                    // Set cache
                    await cache.SetRecordAsync<TokenEndPointResponse>(recordKey, model, TimeSpan.FromSeconds(redis.Expire!.Value));

                    //stop
                    stopwatch.Stop();
                    Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                    return Ok(model);
                }
                else
                {
                    //stop
                    stopwatch.Stop();
                    Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                    return BadRequest(new CommonResponse
                    {
                        Success = false,
                        Code = nameof(ResultCode.GRANT_INVALID),
                        Message = ResultCode.GRANT_INVALID
                    });
                }
            }

            // Check expired token life time
            if (!validateToken(cacheResult.AccessToken))
            {
                if (request.GrantType == "password")
                {

                    // Check password is same as cache
                    if (request.Password.Equals(cacheResult.Password) && request.Username.Equals(cacheResult.Username))
                    {
                        var response = await httpService.Client.RequestPasswordTokenAsync(new PasswordTokenRequest
                        {
                            Address = $"{openId.Authority}/connect/token",
                            GrantType = "password",

                            ClientId = request.ClientID,
                            ClientSecret = request.Secret,

                            UserName = request.Username,
                            Password = request.Password,

                            Scope = request.Scope

                        }, cancellationToken);

                        if (response.IsError)
                        {
                            //stop
                            stopwatch.Stop();
                            Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                            return BadRequest(new CommonResponse
                            {
                                Success = false,
                                Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                                Message = response.Error
                            });
                        }

                        var model = new TokenEndPointResponse
                        {
                            Success = true,
                            AccessToken = response.AccessToken,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Message = ResultCode.SUCCESS_OPERATION,
                            Expire = response.ExpiresIn,
                            RefreshToken = response.RefreshToken,
                            Password = request.Password,
                            Username = request.Username
                        };

                        // Set cache
                        await cache.SetRecordAsync<TokenEndPointResponse>(recordKey, model, TimeSpan.FromSeconds(3600));

                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return Ok(model);
                    }

                    // Return user password is incorrect
                    return BadRequest(new CommonResponse
                    {
                        Success = false,
                        Code = nameof(ResultCode.USERNAME_NOT_FOUND),
                        Message = "User or Password is incorrect"
                    });
                }
                else if (request.GrantType == "client_credentials")
                {
                    var response = await httpService.Client.RequestTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = $"{openId.Authority}/connect/token",
                        GrantType = "client_credentials",

                        ClientId = request.ClientID,
                        ClientSecret = request.Secret,

                        Scope = request.Scope

                    }, cancellationToken);
                    if (response.IsError)
                    {
                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return BadRequest(new CommonResponse
                        {
                            Success = false,
                            Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                            Message = response.Error
                        });
                    }

                    var model = new TokenEndPointResponse
                    {
                        Success = true,
                        AccessToken = response.AccessToken,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION,
                        Expire = response.ExpiresIn,
                        RefreshToken = response.RefreshToken
                    };

                    // Set cache
                    await cache.SetRecordAsync<TokenEndPointResponse>(recordKey, model, TimeSpan.FromSeconds(redis.Expire!.Value));

                    //stop
                    stopwatch.Stop();
                    Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                    return Ok(model);
                }
                else
                {
                    //stop
                    stopwatch.Stop();
                    Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                    return BadRequest(new CommonResponse
                    {
                        Success = false,
                        Code = nameof(ResultCode.GRANT_INVALID),
                        Message = ResultCode.GRANT_INVALID
                    });
                }
            }
            else
            {
                // Check password is same as cache
                if (request.GrantType == "password")
                {
                    if (request.Password.Equals(cacheResult.Password) && request.Username.Equals(cacheResult.Username))
                    {
                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return Ok(new TokenEndPointResponse
                        {
                            Success = true,
                            AccessToken = cacheResult.AccessToken,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Message = ResultCode.SUCCESS_OPERATION,
                            Expire = cacheResult.Expire,
                            RefreshToken = cacheResult.RefreshToken
                        });
                    }
                    else
                    {
                        var response = await httpService.Client.RequestPasswordTokenAsync(new PasswordTokenRequest
                        {
                            Address = $"{openId.Authority}/connect/token",
                            GrantType = "password",

                            ClientId = request.ClientID,
                            ClientSecret = request.Secret,

                            UserName = request.Username,
                            Password = request.Password,

                            Scope = request.Scope

                        }, cancellationToken);

                        if (response.IsError)
                        {
                            //stop
                            stopwatch.Stop();
                            Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                            return BadRequest(new CommonResponse
                            {
                                Success = false,
                                Code = nameof(ResultCode.USERNAME_NOT_FOUND),
                                Message = "User or Password is incorrect"
                            });
                        }

                        var model = new TokenEndPointResponse
                        {
                            Success = true,
                            AccessToken = response.AccessToken,
                            Code = nameof(ResultCode.SUCCESS_OPERATION),
                            Message = ResultCode.SUCCESS_OPERATION,
                            Expire = response.ExpiresIn,
                            RefreshToken = response.RefreshToken,
                            Password = request.Password,
                            Username = request.Username
                        };

                        // Set cache
                        await cache.SetRecordAsync<TokenEndPointResponse>(recordKey, model, TimeSpan.FromSeconds(3600));
                        //stop
                        stopwatch.Stop();
                        Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                        return Ok(model);
                    }
                }
                else
                {
                    //stop
                    stopwatch.Stop();
                    Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                    return Ok(new TokenEndPointResponse
                    {
                        Success = true,
                        AccessToken = cacheResult.AccessToken,
                        Code = nameof(ResultCode.SUCCESS_OPERATION),
                        Message = ResultCode.SUCCESS_OPERATION,
                        Expire = cacheResult.Expire,
                        RefreshToken = cacheResult.RefreshToken
                    });
                }
            }


        }
        private bool validateToken(string token)
        {
            if (token == null) return false;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                return (jwtSecurityToken.ValidTo > DateTime.UtcNow.AddMinutes(-5) ? true : false);
            }
            catch (Exception)
            {
                return false;
            }


        }
        /// <summary>
        /// ແມ່ນ Endpoint ສຳລັບຂໍ Access Token ໃຫມ່ດ້ວຍການສົ່ງ refresh token ເຂົ້າມາ
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("refreshToken")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(TokenEndPointResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshTokenV1([FromBody] RefreshTokenEndPointRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            //start stopwatch
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var response = await httpService.Client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = $"{openId.Authority}/connect/token",

                ClientId = request.ClientID,
                ClientSecret = request.Secret,

                RefreshToken = request.RefreshToken

            }, cancellationToken);

            if (response.IsError)
            {
                //stop
                stopwatch.Stop();
                Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

                return BadRequest(new CommonResponse
                {
                    Success = false,
                    Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                    Message = response.Error
                });
            }

            var model = new TokenEndPointResponse
            {
                Success = true,
                AccessToken = response.AccessToken,
                Code = nameof(ResultCode.SUCCESS_OPERATION),
                Message = ResultCode.SUCCESS_OPERATION,
                Expire = response.ExpiresIn,
                RefreshToken = response.RefreshToken
            };

            //stop
            stopwatch.Stop();
            Console.WriteLine("\n------------Time elapse: " + stopwatch.Elapsed.ToString() + "------------\n");

            return Ok(model);
        }

    }
}
