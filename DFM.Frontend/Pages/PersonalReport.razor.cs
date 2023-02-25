using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using Elasticsearch.Net;
using HttpClientService;
using Minio.DataModel;
using MudBlazor;
using MyCouch;
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
        IEnumerable<TabItemDto>? myRoles;
        string? current;
        string? previousBtn = "";
        string? oldLink = "";
        ReportDrillDownEnum isDrillDown = ReportDrillDownEnum.Search;
        private ReportClickDTO? itemClick = new();
        protected override async Task OnInitializedAsync()
        {
            var rules = await storageHelper.GetRuleMenuAsync();
            if (!ValidateRule.isInRole(rules, $"/pages/report/{Link}"))
            {
                nav.NavigateTo("/pages/unauthorized");
            }

            oldLink = Link!;
            InboxType inboxType = InboxType.Inbound;
            if (Link == "inbound")
            {
                current = "ລາຍງານເອກະສານຂາເຂົ້າ";
                inboxType = InboxType.Inbound;
            }
            else
            {
                current = "ລາຍງານເອກະສານຂາອອກ";
                inboxType = InboxType.Outbound;
            }
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
                tabItems = myRoles!.ToList();
                roleIds = tabItems.Select(x => x.Role.RoleID).ToList()!;

                token = await accessToken.GetTokenAsync();


                onProcessing = true;
                string url = $"{endpoint.API}/api/v1/Document/GetPersonalReport";
                var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, new GetPersonalReportRequest
                {
                    end = -1,
                    start = -1,
                    inboxType = inboxType,
                    roleIDs = roleIds
                }, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                if (result.Success)
                {
                    reportSummary = result.Response;

                }
                onProcessing = false;
            }

            await InvokeAsync(StateHasChanged);
        }
        protected override async Task OnParametersSetAsync()
        {
            if (oldLink != Link)
            {
                InboxType inboxType = InboxType.Inbound;
                isDrillDown = ReportDrillDownEnum.Search;
                oldLink = Link!;
                reportSummary = new();
                searchRequest = new();
                documentModel = new();
                rawDocument = new();
                if (Link == "inbound")
                {
                    current = "ລາຍງານເອກະສານຂາເຂົ້າ";
                    inboxType = InboxType.Inbound;
                }
                else
                {
                    current = "ລາຍງານເອກະສານຂາອອກ";
                    inboxType = InboxType.Outbound;
                }
                onProcessing = true;
                string url = $"{endpoint.API}/api/v1/Document/GetPersonalReport";
                var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, new GetPersonalReportRequest
                {
                    end = -1,
                    start = -1,
                    inboxType = inboxType,
                    roleIDs = roleIds
                }, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                if (result.Success)
                {
                    reportSummary = result.Response;

                }
                onProcessing = false;
            }
            base.OnParametersSet();
        }
        private async Task onSearch(GetPersonalReportRequest callback)
        {

            searchRequest = callback;
            onProcessing = true;
            string url = $"{endpoint.API}/api/v1/Document/GetPersonalReport";
            var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, callback, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

            if (result.Success)
            {
                reportSummary = result.Response;

            }
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
            var result = await httpService.Post<GetPersonalReportRequest, List<PersonalReportSummary>>(url, searchRequest, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
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

        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
        void notifyMessage(string content)
        {
            AlertMessage(content, Defaults.Classes.Position.BottomRight, Severity.Warning);
        }
    }
}
