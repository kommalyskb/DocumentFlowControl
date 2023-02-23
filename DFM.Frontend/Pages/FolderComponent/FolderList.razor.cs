using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.FolderComponent
{
    public partial class FolderList
    {
        //string? token = "";
        int _panelIndex = 0;
        string? oldLink = "";
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role.RoleID); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        private IEnumerable<TabItemDto>? allTabs;

        async Task onRowClick(FolderModel item)
        {
            await OnRowClick.InvokeAsync(item);
        }
        protected override async Task OnInitializedAsync()
        {
            oldLink = Link;
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }

            if (allTabs!.IsNullOrEmpty())
            {
                allTabs = await storageHelper.GetRolesAsync();
                
            }

            if (Link == "outbound")
            {
                if (!allTabs!.IsNullOrEmpty())
                {
                    tabItems = allTabs!.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();

                }

            }
            else
            {
                if (!allTabs!.IsNullOrEmpty())
                {
                    tabItems = allTabs!.Where(x => x.Role.RoleType != RoleTypeModel.OutboundPrime && x.Role.RoleType != RoleTypeModel.OutboundOfficePrime && x.Role.RoleType != RoleTypeModel.OutboundGeneral).ToList();

                }
            }
            if (!tabItems!.IsNullOrEmpty())
            {
                // Callback event 
                await OnTabChangeEvent.InvokeAsync(tabItems![_panelIndex].Role.RoleID);

            }

        }
        protected override async Task OnParametersSetAsync()
        {
            if (oldLink != Link)
            {
                oldLink = Link;
                if (Link == "outbound")
                {
                    if (!allTabs!.IsNullOrEmpty())
                    {
                        tabItems = allTabs!.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();

                    }

                }
                else
                {
                    if (!allTabs!.IsNullOrEmpty())
                    {
                        tabItems = allTabs!.Where(x => x.Role.RoleType != RoleTypeModel.OutboundPrime && x.Role.RoleType != RoleTypeModel.OutboundOfficePrime && x.Role.RoleType != RoleTypeModel.OutboundGeneral).ToList();

                    }
                }
                if (!tabItems!.IsNullOrEmpty())
                {
                    // Callback event 
                    await OnTabChangeEvent.InvokeAsync(tabItems![_panelIndex].Role.RoleID);

                }
            }
        }
    }
}
