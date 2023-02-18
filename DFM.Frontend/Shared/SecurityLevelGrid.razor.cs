using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Shared
{
    public partial class SecurityLevelGrid
    {

        private List<string> _events = new();
        string? token = "";
        private EmployeeModel? employee;
        private List<DocumentSecurityModel> Elements = new();
        // events
        async Task RowClicked(DataGridRowClickEventArgs<DocumentSecurityModel> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            await OnRowClick.InvokeAsync(args.Item);
        }

        void SelectedItemsChanged(HashSet<DocumentSecurityModel> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<DocumentSecurityModel, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<DocumentSecurityModel, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load document
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                string url = $"{endpoint.API}/api/v1/SecurityLevel/GetItems/{employee.OrganizationID}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                var result = await httpService.Get<IEnumerable<DocumentSecurityModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    Elements = result.Response.ToList();
                }
            }
            catch (Exception)
            {

            }

        }
    }
}
