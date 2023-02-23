using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using HttpClientService;

namespace DFM.Frontend.Pages.SecurityLevelComponent
{
    public partial class SecurityList
    {
        //string? token = "";
        int _panelIndex = 0;
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role.RoleID); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        IEnumerable<TabItemDto>? myRoles;
        async Task onRowClick(DocumentSecurityModel item)
        {
            await OnRowClick.InvokeAsync(item);
        }
        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }

            if (myRoles!.IsNullOrEmpty())
            {
                myRoles = await storageHelper.GetRolesAsync();

                
            }
            if (!myRoles!.IsNullOrEmpty())
            {
                tabItems = myRoles!.ToList();

                // Callback event 
                await OnTabChangeEvent.InvokeAsync(tabItems![_panelIndex].Role.RoleID);
            }
           
        }
    }
}
