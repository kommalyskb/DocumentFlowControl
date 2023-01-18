using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages
{
    public partial class Index
    {
        private EmployeeModel? employee;
        private IEnumerable<TabItemDto>? roles;
        IEnumerable<string>? selectValues;
        protected override async Task OnInitializedAsync()
        {

            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Load tab
            string url = $"{endpoint.API}/api/v1/Organization/GetRole";


            string token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                roles = result.Response;
                selectValues = roles.Select(x => x.Role.RoleID);


            }
            await InvokeAsync(StateHasChanged);

        }
        
    }
}
