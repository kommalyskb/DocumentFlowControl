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

            if (roles == null)
            {
                roles = await storageHelper.GetRolesAsync();
               
            }
            roleID = roles!.FirstOrDefault()!.Role!.RoleID!;
            selectValues = new List<string>() { roleID };

            await InvokeAsync(StateHasChanged);

        }

    }
}
