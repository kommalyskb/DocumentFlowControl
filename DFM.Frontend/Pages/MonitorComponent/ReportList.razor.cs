﻿using DFM.Shared.Common;
using DFM.Shared.DTOs;

namespace DFM.Frontend.Pages.MonitorComponent
{
    public partial class ReportList
    {
        private async Task onSearch()
        {
            if (string.IsNullOrWhiteSpace(startDate))
            {
                await NotifyMessage.InvokeAsync("ຜິດພາດ, ກະລຸນາປ້ອນວັນທີ່ເລີ່ມຕົ້ນຄົ້ນຫາ");
                return;
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                await NotifyMessage.InvokeAsync("ຜິດພາດ, ກະລຸນາປ້ອນວັນທີ່ສິ້ນສຸດຄົ້ນຫາ");
                return;
            }
            GetPersonalReportRequest callBack = new GetPersonalReportRequest
            {
                inboxType = InboxType,
                roleIDs = RoleIDs
            };
            var startDateItems = startDate!.Split("/");
            var endDateItems = endDate!.Split("/");

            callBack.start = Convert.ToDecimal($"{startDateItems[2]}{startDateItems[1]}{startDateItems[0]}000000");
            callBack.end = Convert.ToDecimal($"{endDateItems[2]}{endDateItems[1]}{endDateItems[0]}000000");

            await OnSearch.InvokeAsync(callBack);
        }

        async Task onReportClick(TraceStatus status, string roleId)
        {
            Console.WriteLine($"Status: {status}, RoleID: {roleId}");
            await OnReportItemClick.InvokeAsync(new ReportClickDTO
            {
                TraceStatus = status,
                RoleID = roleId
            });
        }
    }
}
