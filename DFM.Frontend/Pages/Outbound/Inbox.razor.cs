using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.Outbound
{
    public partial class Inbox
    {
        string? token = "";
        int _panelIndex = 0;
        int panelIndex { get { return _panelIndex; } set { _panelIndex = value; OnTabChangeEvent.InvokeAsync(tabItems![value].Role); } }
        private EmployeeModel? employee;
        List<TabItemDto>? tabItems;
        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Load tab
            string url = $"{endpoint.API}/api/v1/Organization/GetRole";


            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token));

            tabItems = result.Response.Where(x => x.Role.RoleType != RoleTypeModel.InboundPrime && x.Role.RoleType != RoleTypeModel.InboundOfficePrime && x.Role.RoleType != RoleTypeModel.InboundGeneral).ToList();

            // Callback event 
            await OnTabChangeEvent.InvokeAsync(tabItems[_panelIndex].Role);
        }

        async Task onRowClick(DocumentModel item)
        {
            await OnRowClick.InvokeAsync(item);
        }

    }
}
