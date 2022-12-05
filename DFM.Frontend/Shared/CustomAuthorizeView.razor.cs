using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.DTOs;
using DFM.Shared.Helper;
using HttpClientService;
using Microsoft.AspNetCore.Components;

namespace DFM.Frontend.Shared
{
    public partial class CustomAuthorizeView
    {
        [Parameter] public RenderFragment? Authorized { get; set; }
        [Parameter] public RenderFragment? NotAuthorized { get; set; }
        [Parameter] public RenderFragment? Authorizing { get; set; }
        [Inject] AccessTokenStorage accessToken { get; set; }
        [Inject] public IHttpService httpService { get; set; }
        [Inject] public ServiceEndpoint endpoint { get; set; }

        string token = "";
        public int State = 0;
        protected override async Task OnInitializedAsync()
        {
            await CheckAuth();
        }
        int counter = 0;
        private async Task CheckAuth()
        {
            State = 0;
            await InvokeAsync(StateHasChanged);


            token = await accessToken.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                State = 1;
                await InvokeAsync(StateHasChanged);
            }
            else if (!AccessTokenService.CheckToken(token))
            {
                string refreshToken = await accessToken.GetRefreshTokenAsync();
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshTokenReq = new RefreshTokenEndPointRequest
                    {
                        ClientID = "dfm_frontend",
                        RefreshToken = refreshToken,
                        Secret = "a401b0a6-59e0-2255-513f-4b7c77bba237"
                    };
                    var refreshTokenResult = await httpService.Post<RefreshTokenEndPointRequest, TokenEndPointResponse, CommonResponse>($"{endpoint.API}/api/v1/Connect/refreshToken", refreshTokenReq);
                    if (refreshTokenResult.Success)
                    {
                        await accessToken.SetTokenAsync(refreshTokenResult.Response.AccessToken!);
                        await accessToken.SetRefreshTokenAsync(refreshTokenResult.Response.RefreshToken!);


                        State = 2;
                        await InvokeAsync(StateHasChanged);
                    }
                    else
                    {
                        if (counter >= 1)
                        {
                            await accessToken.RemoveTokenAsync();

                            State = 1;
                            await InvokeAsync(StateHasChanged);
                        }
                        else
                        {
                            counter++;
                            await CheckAuth();
                        }

                    }
                }
            }
            else
            {

                State = 2;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
