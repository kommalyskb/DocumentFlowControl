using DFM.Shared.DTOs;

namespace DFM.Frontend.Pages.ReportComponent
{
    public partial class ReportList
    {
        private async Task onSearch()
        {

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
    }
}
