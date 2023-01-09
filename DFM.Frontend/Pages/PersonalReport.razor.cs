using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages
{
    public partial class PersonalReport
    {
        private EmployeeModel? employee;
        string? token;
        private List<TabItemDto> tabItems = new();
        private PersonalReportSummary reportSummary = new();

        protected override async Task OnInitializedAsync()
        {

            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Load tab
            string url = $"{endpoint.API}/api/v1/Organization/GetRole?fakeId={employee.id}";
            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<TabItemDto>>(url, new AuthorizeHeader("bearer", token));

            //tabItems = result.Response.ToList();
            roleIds = result.Response.Select(x => x.Role.RoleID).ToList()!;
        }
        private async Task onSearch(GetPersonalReportRequest callback)
        {
            string url = $"{endpoint.API}/api/v1/Document/GetPersonalReport";
            var result = await httpService.Post<GetPersonalReportRequest, PersonalReportSummary>(url, callback, new AuthorizeHeader("bearer", token));

            reportSummary = result.Response;
            await InvokeAsync(StateHasChanged);

        }
    }
}
