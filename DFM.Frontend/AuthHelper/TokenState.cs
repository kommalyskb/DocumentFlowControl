using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Helper;
using HttpClientService;
using System.Diagnostics.Metrics;

namespace DFM.Frontend.AuthHelper
{
    public class TokenState
    {
        private readonly AccessTokenStorage accessToken;
        private readonly FrontendIdentity identity;
        private readonly IHttpService httpService;
        private readonly ServiceEndpoint endpoint;

        public TokenState(AccessTokenStorage accessToken, FrontendIdentity identity, IHttpService httpService, ServiceEndpoint endpoint)
        {
            this.accessToken = accessToken;
            this.identity = identity;
            this.httpService = httpService;
            this.endpoint = endpoint;
        }
        public async Task<bool> ValidateToken()
        {
            var token = await accessToken.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                // No refress Token
                return false;
            }
            else
            {
                if (!AccessTokenService.CheckToken(token))
                {
                    var refreshToken = await accessToken.GetRefreshTokenAsync();
                    var refreshTokenReq = new RefreshTokenEndPointRequest
                    {
                        ClientID = identity!.ClientID,
                        RefreshToken = refreshToken,
                        Secret = identity.Secret
                    };
                    var refreshTokenResult = await httpService!.Post<RefreshTokenEndPointRequest, TokenEndPointResponse, CommonResponse>($"{endpoint!.API}/api/v1/Connect/refreshToken", refreshTokenReq);
                    if (refreshTokenResult.Success)
                    {
                        await Task.WhenAll(accessToken.SetTokenAsync(refreshTokenResult.Response.AccessToken!), accessToken.SetRefreshTokenAsync(refreshTokenResult.Response.RefreshToken!));

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
