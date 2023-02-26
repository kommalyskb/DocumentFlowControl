using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.FreeFlow
{
    public partial class FreeFlowList
    {
        string? token = "";
        private List<DynamicItem> Elements = new();
        private EmployeeModel? employee;
        async Task RowClicked(DataGridRowClickEventArgs<DynamicItem> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            await OnRowClick.InvokeAsync(args.Item);
        }

        void SelectedItemsChanged(HashSet<DynamicItem> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<DynamicItem, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<DynamicItem, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };

        protected override async Task OnInitializedAsync()
        {
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            // Load document
            string url = $"{endpoint.API}/api/v1/Organization/GetDynamicFlow/{ModuleType}/{employee.OrganizationID}";
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }

            var result = await httpService.Get<IEnumerable<DynamicItem>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (result.Success)
            {
                Elements = result.Response!.ToList();
            }
        }
    }
}
