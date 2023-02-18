using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;

namespace DFM.Frontend.Pages.UrgentLevelComponent
{
    public partial class UrgentList
    {
        //string? token = "";
        int _panelIndex = 0;
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role.RoleID); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        IEnumerable<TabItemDto>? myRoles;
        async Task onRowClick(DocumentUrgentModel item)
        {
            await OnRowClick.InvokeAsync(item);
        }
        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            if (myRoles == null)
            {
                myRoles = await storageHelper.GetRolesAsync();

                tabItems = myRoles.ToList();

                // Callback event 
                await OnTabChangeEvent.InvokeAsync(tabItems[_panelIndex].Role.RoleID);
            }

            
        }
    }
}
