using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Shared
{
    public partial class DocumentTypeGrid
    {
        private List<string> _events = new();
        string? token = "";
        private EmployeeModel? employee;
        private List<DataTypeModel> Elements = new();
        // events
        async Task RowClicked(DataGridRowClickEventArgs<DataTypeModel> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            await OnRowClick.InvokeAsync(args.Item);
        }

        void SelectedItemsChanged(HashSet<DataTypeModel> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<DataTypeModel, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<DataTypeModel, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };
        protected override async Task OnInitializedAsync()
        {
            // Load document
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            string url = $"{endpoint.API}/api/v1/DocumentType/GetItems/{employee.OrganizationID}";
            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<DataTypeModel>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                Elements = result.Response.ToList();
            }
        }

    }
}
