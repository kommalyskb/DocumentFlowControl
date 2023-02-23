using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using HttpClientService;

namespace DFM.Frontend.Pages.Inbound
{
    public partial class Inbox
    {
        //string? token = "";
        int _panelIndex = 0;
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        IEnumerable<TabItemDto>? myRoles;
        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Load tab
            if (myRoles!.IsNullOrEmpty())
            {
                myRoles = await storageHelper.GetRolesAsync();

            }
            if (!myRoles!.IsNullOrEmpty())
            {
                tabItems = myRoles!.Where(x => x.Role.RoleType != RoleTypeModel.OutboundPrime && x.Role.RoleType != RoleTypeModel.OutboundOfficePrime && x.Role.RoleType != RoleTypeModel.OutboundGeneral).ToList();
                if (tabItems.Count > 0)
                {
                    // Callback event 
                    await OnTabChangeEvent.InvokeAsync(tabItems![_panelIndex].Role);
                }
            }
            

        }
        async Task onRowClick(DocumentModel item)
        {
            await OnRowClick.InvokeAsync(item);
        }

    }
}
