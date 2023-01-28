using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using System.Linq;

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
                roleID = roles!.FirstOrDefault()!.Role!.RoleID!;
                selectValues = new List<string>() { roleID };
                
            }
            await InvokeAsync(StateHasChanged);

        }

    }
}
