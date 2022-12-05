using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages
{
    public partial class Index
    {
        IEnumerable<RoleTreeModel>? demoUser;
        protected override async Task OnInitializedAsync()
        {
            string token = await accessToken.GetTokenAsync();


            string url = $"{endpoint.API}/api/v1/Organization/GetItem/b98c5c46cebd430bb7d9fe596d73c459";
            var orgResult = await httpService.Get<IEnumerable<RoleTreeModel>>(url, new AuthorizeHeader("bearer", token));
            if (orgResult.Success)
            {
                demoUser = orgResult.Response;
            }

            
        }
        private async Task startProgram()
        {
            selectModel = demoUser.FirstOrDefault(x => x.Role.RoleID == roleId);

            string token = await accessToken.GetTokenAsync();

            var employeeProfile = await cascading.GetEmployeeProfile(token, selectModel.Employee.UserID);

            await storageHelper.SetEmployeeProfileAsync(employeeProfile.Content);
        }
    }
}
