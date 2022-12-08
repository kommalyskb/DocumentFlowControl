using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Shared
{
    public partial class UrgentLevelGrid
    {
        private List<string> _events = new();
        string? token = "";
        private List<DocumentUrgentModel> Elements = new();
        // events
        async Task RowClicked(DataGridRowClickEventArgs<DocumentUrgentModel> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            await OnRowClick.InvokeAsync(args.Item);
        }

        void SelectedItemsChanged(HashSet<DocumentUrgentModel> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<DocumentUrgentModel, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<DocumentUrgentModel, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };
        protected override async Task OnInitializedAsync()
        {
            // Load document
            var employee = await storageHelper.GetEmployeeProfileAsync();
            string url = $"{endpoint.API}/api/v1/UrgentLevel/GetItems/{employee.OrganizationID}";
            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<DocumentUrgentModel>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                Elements = result.Response.ToList();
            }
        }
    }
}
