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
            await Task.WhenAll(accessToken.RemoveTokenAsync(), storageHelper.RemoveCacheAsync());
        }
        private async Task OnValidSubmit()
        {
            onProcessing = true;
            string url = $"{endpoint.API}/api/v1/Connected/token";
            httpService.MediaType = MediaType.JSON;
            if (model.Username != "admin")
            {
                model.Password = $"{model.Password}@Dfm.codecamp";
            }
            TokenEndPointRequest request = new TokenEndPointRequest
            {
                Username = model.Username,
                Password = model.Password,
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
                // Set access token to local storage
                
                // Set refresh token to local storage

                // Get Profile to local storage
                var employeeProfile = await cascading.GetEmployeeProfile(result.Response.AccessToken!);
                if (!employeeProfile.Success)
                {
                    AlertMessage($"ບໍ່ສາມາດ ລັອກອິນໄດ້, ພົບບັນຫາກ່ຽວກັບຂໍ້ມູນຜູ້ໃຊ້", Defaults.Classes.Position.BottomRight, Severity.Error);
                }
                else
                {
                    // Get Roles to local storage
                    var roles = await cascading.GetRoles(result.Response.AccessToken!);
                    // Get Rules Menu to local storage
                    var rules = await cascading.GetRules(result.Response.AccessToken!);

                    await Task.WhenAll(accessToken.SetTokenAsync(result.Response.AccessToken),
                        accessToken.SetRefreshTokenAsync(result.Response.RefreshToken),
                        storageHelper.SetEmployeeProfileAsync(employeeProfile.Content),
                        storageHelper.SetRolesAsync(roles.Contents),
                        storageHelper.SetRuleMenuAsync(rules.Contents));

                    if (model.Username == "admin")
                    {
                        nav.NavigateTo("/setup", true);
                    }
                    else
                    {
                        nav.NavigateTo("/", true);

                    }
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
