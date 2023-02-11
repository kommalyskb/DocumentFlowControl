using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.FolderComponent
{
    public partial class FolderList
    {
        string? token = "";
        int _panelIndex = 0;
        string? oldLink = "";
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role.RoleID); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        private IEnumerable<TabItemDto> allTabs;

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
            // Load tab
            string url = $"{endpoint.API}/api/v1/Organization/GetRole";


            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                allTabs = result.Response;
                if (Link == "outbound")
                {
                    tabItems = allTabs.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();

                }
                else
                {
                    tabItems = allTabs.Where(x => x.Role.RoleType != RoleTypeModel.OutboundPrime && x.Role.RoleType != RoleTypeModel.OutboundOfficePrime && x.Role.RoleType != RoleTypeModel.OutboundGeneral).ToList();
                }
                if (tabItems.Count > 0)
                {
                    // Callback event 
                    await OnTabChangeEvent.InvokeAsync(tabItems[_panelIndex].Role.RoleID);

                }
            }
            
        }
        protected override async Task OnParametersSetAsync()
        {
            if (oldLink != Link)
            {
                oldLink = Link;
                if (Link == "outbound")
                {
                    tabItems = allTabs.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();

                }
                else
                {
                    tabItems = allTabs.Where(x => x.Role.RoleType != RoleTypeModel.OutboundPrime && x.Role.RoleType != RoleTypeModel.OutboundOfficePrime && x.Role.RoleType != RoleTypeModel.OutboundGeneral).ToList();
                }
                if (tabItems.Count > 0)
                {
                    // Callback event 
                    await OnTabChangeEvent.InvokeAsync(tabItems[_panelIndex].Role.RoleID);

                }
            }
        }
    }
}
