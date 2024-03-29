﻿using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Extensions;
using DFM.Shared.Resources;
using HttpClientService;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public interface IIdentityHelper
    {
        Task<(string Token, CommonResponse Response)> GetAdminAccessToken();
        bool ValidateToken(string token);
        Task<(int SearchState, string UserID)> ValidateUser(string username, string token);
    }
    public class IdentityHelper : IIdentityHelper
    {
        private readonly IHttpService httpService;
        private readonly ServiceEndpoint endpoint;
        private readonly IRedisConnector redisConnector;
        private readonly OpenIDConf openID;
        private readonly ILogger<IdentityHelper> logger;

        public IdentityHelper(IHttpService httpService, ServiceEndpoint endpoint, IRedisConnector redisConnector, OpenIDConf openID, ILogger<IdentityHelper> logger)
        {
            this.httpService = httpService;
            this.endpoint = endpoint;
            this.redisConnector = redisConnector;
            this.openID = openID;
            this.logger = logger;
        }
        public bool ValidateToken(string token)
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
        public async Task<(int SearchState, string UserID)> ValidateUser(string username, string token)
        {
            try
            {
                int found = 2;
                string id = "";
                string url = $"{endpoint.IdentityAPI}/api/Users?searchText={username}&page=1&pageSize=10";
                var validResult = await httpService.Get<object>(url, new AuthorizeHeader("bearer", token));

                var validContent = await validResult.HttpResponseMessage.Content.ReadAsStringAsync();
                Log.Information($"Search User: {validContent}");
                Log.Information($"Token: {token}");
                Log.Information($"Url: {url}");

                if (!validResult.Success)
                {
                    found = 0;
                }
                else
                {
                    
                    var validDeserialize = JsonSerializer.Deserialize<UserSearchResponse>(validContent);

                    if (validDeserialize?.totalCount == 0)
                    {
                        found = 1;
                    }
                    else
                    {
                        id = validDeserialize!.users![0].id!;
                    }


                }

                return (found, id);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<(string Token, CommonResponse Response)> GetAdminAccessToken()
        {
            string token = "";
            bool isNull = false;
            CommonResponse res = new();
            var provider = new RedisConnectionProvider(redisConnector.Connection);
            var response = provider.RedisCollection<TokenEndPointResponse>();
            
            var admin = await response.FindByIdAsync("admin");
            if (admin != null)
            {
                logger.LogInformation($"Admin: {admin.AccessToken}");
                isNull = false;
                if (ValidateToken(admin.AccessToken!))
                {
                    token = admin.AccessToken!;
                    res = new CommonResponse
                    {
                        Success = admin.Success,
                        Code = admin.Code,
                        Message = admin.Message,
                        Detail = admin.Detail
                    };

                    return (token!, res);
                }
            }
            else
            {
                isNull = true;
            }
            var tokenResult = await httpService.Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = $"{openID.Authority}/connect/token",
                GrantType = "password",
                ClientId = openID.AdminClient, // Create client with scope DFMClient_api
                ClientSecret = openID.AdminSecret,
                UserName = openID.AdminUsername,
                Password = openID.AdminPassword,
                Scope = openID.AdminScope
            });
            
            if (!tokenResult.IsError)
            {
                admin = new TokenEndPointResponse
                {
                    Success = true,
                    AccessToken = tokenResult.AccessToken,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION,
                    Expire = tokenResult.ExpiresIn,
                    RefreshToken = tokenResult.RefreshToken,
                    Username = openID.AdminUsername,
                    Password = openID.AdminPassword,
                    Detail = ResultCode.SUCCESS_OPERATION
                };
                token = tokenResult.AccessToken;
                res = new()
                {
                    Success = true,
                    Code = nameof(ResultCode.SUCCESS_OPERATION),
                    Message = ResultCode.SUCCESS_OPERATION,
                    Detail = ResultCode.SUCCESS_OPERATION
                };
                //update or insert
                if (!isNull)
                {
                    await response.UpdateAsync(admin);
                }
                else
                {
                    await response.InsertAsync(admin);
                }
            }
            else
            {
                logger.LogError($"Error Request Admin Authorize: {tokenResult.ErrorDescription}");
                res = new CommonResponse
                {
                    Success = false,
                    Code = nameof(ResultCode.REQUEST_TOKEN_ERROR),
                    Message = tokenResult.Error,
                    Detail = tokenResult.Error
                };
            }
            
            return (token, res);
        }
    }
}
