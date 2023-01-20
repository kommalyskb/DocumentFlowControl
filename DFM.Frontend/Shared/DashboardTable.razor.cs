﻿using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace DFM.Frontend.Shared
{
    public partial class DashboardTable
    {
        private EmployeeModel? employee;
        private string? token;
        private PersonalReportSummary? summary = new();

        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            token = await accessToken.GetTokenAsync();

            await loadContent();

            await InvokeAsync(StateHasChanged);

        }

        protected override async Task OnParametersSetAsync()
        {
            await loadContent();

            await InvokeAsync(StateHasChanged);
        }

        private async Task loadContent()
        {
            string url = $"{endpoint.API}/api/v1/Document/GetDashboard";
            var result = await httpService.Post<GetDashboardRequest, PersonalReportSummary>(url, new GetDashboardRequest
            {
                inboxType = InboxType,
                roleID = RoleId
            }, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                summary = result.Response;
            }
        }
    }
}