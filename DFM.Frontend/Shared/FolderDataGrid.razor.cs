using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Shared
{
    public partial class FolderDataGrid
    {
        private List<string> _events = new();
        string? token = "";
        private List<FolderModel> Elements = new();
        string? oldLink = "";
        // events
        async Task RowClicked(DataGridRowClickEventArgs<FolderModel> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            await OnRowClick.InvokeAsync(args.Item);
        }

        void SelectedItemsChanged(HashSet<FolderModel> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<FolderModel, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<FolderModel, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };
        protected override async Task OnInitializedAsync()
        {
            oldLink = Link;
            // Load document
            string url = $"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}/{Link}";
            token = await accessToken.GetTokenAsync();

            var result = await httpService.Get<IEnumerable<FolderModel>>(url, new AuthorizeHeader("bearer", token));
            if (result.Success)
            {
                Elements = result.Response.OrderBy(x => x.Seq).ToList();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (oldLink != Link)
            {
                oldLink = Link;
                // Load document
                string url = $"{endpoint.API}/api/v1/Folder/GetItems/{RoleId}/{Link}";
                token = await accessToken.GetTokenAsync();

                var result = await httpService.Get<IEnumerable<FolderModel>>(url, new AuthorizeHeader("bearer", token));
                if (result.Success)
                {
                    Elements = result.Response.OrderBy(x => x.Seq).ToList();
                }
                else
                {
                    Elements.Clear();
                }
            }
        }
    }
}
