using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.Menu
{
    public partial class MenuList
    {
        string? token = "";
        private List<RuleMenu> Elements = new();
        private EmployeeModel? employee;
        private IEnumerable<RuleMenu>? allRules;
        async Task RowClicked(DataGridRowClickEventArgs<RuleMenu> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            var item = allRules!.FirstOrDefault(z => z.id == args.Item.id);
            await OnRowClick.InvokeAsync(item);
        }

        void SelectedItemsChanged(HashSet<RuleMenu> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<RuleMenu, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<RuleMenu, string> _headerStyleFunc => x =>
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
            string url = $"{endpoint.API}/api/v1/RuleMenu/GetRules/{employee.OrganizationID}";
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }

            var result = await httpService.Get<IEnumerable<RuleMenu>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (result.Success)
            {
                allRules = result.Response;
                Elements = result.Response.ToList();
            }
        }

    }
}
