using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.Outbound
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
            if (myRoles!.IsNullOrEmpty())
            {
                myRoles = await storageHelper.GetRolesAsync();

            }
            if (!myRoles!.IsNullOrEmpty())
            {
                tabItems = myRoles!.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();
                if (!tabItems!.IsNullOrEmpty())
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
