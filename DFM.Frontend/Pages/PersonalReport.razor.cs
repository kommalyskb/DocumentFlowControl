using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using Elasticsearch.Net;
using HttpClientService;
using Minio.DataModel;
using MudBlazor;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using static MudBlazor.CategoryTypes;

namespace DFM.Frontend.Pages
{
    public partial class PersonalReport
    {
        private EmployeeModel? employee;
        string? token;
        private List<TabItemDto> tabItems = new();
        private List<PersonalReportSummary> reportSummary = new();
        string? current;
        string? previousBtn = "";
        string? oldLink = "";
        ReportDrillDownEnum isDrillDown = ReportDrillDownEnum.Search;
        private ReportClickDTO? itemClick = new();
        protected override async Task OnInitializedAsync()
        {
            oldLink = Link!;
            if (Link == "inbound")
            {
                current = "ລາຍງານເອກະສານຂາເຂົ້າ";
            }
            else
            {
                current = "ລາຍງານເອກະສານຂາອອກ";
            }
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
        protected override void OnParametersSet()
        {
            if (oldLink != Link)
            {
                isDrillDown = ReportDrillDownEnum.Search;
                oldLink = Link!;
                reportSummary = new();
                searchRequest = new();
                documentModel = new();
                rawDocument = new();
            }
            base.OnParametersSet();
        }
        private async Task onSearch(GetPersonalReportRequest callback)
        {

            searchRequest = callback;
            onProcessing = true;
            string url = $"{endpoint.API}/api/v1/Document/GetPersonalReport";
            var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, callback, new AuthorizeHeader("bearer", token));

            reportSummary = result.Response;
            onProcessing = false;
            await InvokeAsync(StateHasChanged);

        }

        private async Task onItemReportClick(ReportClickDTO item)
        {
            if (Link == "inbound")
            {
                previousBtn = "ລາຍງານເອກະສານຂາເຂົ້າ";
            }
            else
            {
                previousBtn = "ລາຍງານເອກະສານຂາອອກ";
            }
            current = "ລາຍການເອກະສານ";
            itemClick = item;
            searchRequest!.roleIDs = new List<string> { item.RoleID! };
            onProcessing = true;
            string url = $"{endpoint.API}/api/v1/Document/DrillDownReport/{item.TraceStatus}";
            var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, searchRequest, new AuthorizeHeader("bearer", token));
            isDrillDown = ReportDrillDownEnum.List;
            roleId = item.RoleID;
            onProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
        async Task onPrevoiusToSearch()
        {
            previousBtn = "";
            if (Link == "inbound")
            {
                current = "ລາຍງານເອກະສານຂາເຂົ້າ";
            }
            else
            {
                current = "ລາຍງານເອກະສານຂາອອກ";
            }

            isDrillDown = ReportDrillDownEnum.Search;
            await InvokeAsync(StateHasChanged);
        }

        async Task onPrevoiusToList()
        {
            previousBtn = "ລາຍການເອກະສານ";
            if (Link == "inbound")
            {
                current = "ເອກະສານຂາເຂົ້າ";
            }
            else
            {
                current = "ເອກະສານຂາອອກ";
            }

            isDrillDown = ReportDrillDownEnum.List;
            await InvokeAsync(StateHasChanged);
        }

        async Task onRowClick(DocumentModel item)
        {

            // Row click
            documentModel = item;
            var myRole = documentModel!.Recipients!.LastOrDefault(x => x.RecipientInfo.RoleID == roleId);
            rawDocument = documentModel!.RawDatas!.LastOrDefault(x => x.DataID == myRole!.DataID);
            isDrillDown = ReportDrillDownEnum.Detail;
            await InvokeAsync(StateHasChanged);
        }
    }
}
