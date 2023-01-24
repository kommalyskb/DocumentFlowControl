using Confluent.Kafka;
using DFM.Shared.Common;
using DFM.Shared.DTOs;
using HttpClientService;
using MudBlazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DFM.Frontend.Pages
{
    public partial class Login
    {
        protected override async Task OnInitializedAsync()
        {
            await accessToken.RemoveTokenAsync();
        }
        private async Task authorize()
        {
            onProcessing = true;
            string url = $"{endpoint.API}/api/v1/Connected/token";
            httpService.MediaType = MediaType.JSON;
            if (userName != "admin")
            {
                password = $"{password}@Dfm.codecamp";
            }
            TokenEndPointRequest request = new TokenEndPointRequest
            {
                Username = userName,
                Password = password,
                ClientID = identity.ClientID, // create client for production
                GrantType = "password",
                Secret = identity.Secret, // Get secret
                Scope = identity.Scope
            };
            Console.WriteLine("--------Connect Token---------");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));
            var result = await httpService.Post<TokenEndPointRequest, TokenEndPointResponse>(url, request);
            onProcessing = false;
            Console.WriteLine(await result.HttpResponseMessage.Content.ReadAsStringAsync());
            if (result.Success)
            {
                await accessToken.SetTokenAsync(result.Response.AccessToken);
                await accessToken.SetRefreshTokenAsync(result.Response.RefreshToken);

                var employeeProfile = await cascading.GetEmployeeProfile(result.Response.AccessToken);

                await storageHelper.SetEmployeeProfileAsync(employeeProfile.Content);

                if (userName == "admin")
                {
                    nav.NavigateTo("/setup", true);
                }
                else
                {
                    nav.NavigateTo("/", true);

                }

            }
            else
            {
                AlertMessage("ບໍ່ສາມາດ ລັອກອິນໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
        }
        
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
    }
}
