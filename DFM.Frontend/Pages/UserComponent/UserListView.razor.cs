using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserListView
    {
        string? token = "";
        private List<EmployeeDto> Elements = new();
        private EmployeeModel? employee;
        private IEnumerable<EmployeeModel>? allEmployees;
        async Task RowClicked(DataGridRowClickEventArgs<EmployeeDto> args)
        {
            //_events.Insert(0, $"Event = RowClick, Index = {args.RowIndex}, Data = {JsonSerializer.Serialize(args.Item)}");
            //Console.WriteLine(JsonSerializer.Serialize(args.Item));

            // Open new page
            var item = allEmployees!.FirstOrDefault(z => z.id == args.Item.Id);
            await OnRowClick.InvokeAsync(item);
        }

        void SelectedItemsChanged(HashSet<EmployeeDto> items)
        {
            //_events.Insert(0, $"Event = SelectedItemsChanged, Data = {JsonSerializer.Serialize(items)}");
            //Console.WriteLine(JsonSerializer.Serialize(items));
        }
        private Func<EmployeeDto, string> _cellStyleFunc => x =>
        {
            string style = "";

            //if (!x.IsRead)
            //    style += "font-weight:bold";

            return style;
        };
        private Func<EmployeeDto, string> _headerStyleFunc => x =>
        {
            return "font-weight:bold";
        };

        protected override async Task OnInitializedAsync()
        {
            employee = await storageHelper.GetEmployeeProfileAsync();
            // Load document
            string url = $"{endpoint.API}/api/v1/Employee/GetItems/{employee.OrganizationID}";
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }

            var result = await httpService.Get<IEnumerable<EmployeeModel>>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
            if (result.Success)
            {
                allEmployees = result.Response;
                Elements = result.Response.Select(x => new EmployeeDto
                {
                    Email = x.Contact.Email,
                    EmployeeID = x.EmployeeID,
                    Fullname = $"{x.Name.Local} {x.FamilyName.Local}",
                    Gender = x.Gender == Gender.Male ? "ທ່ານ" : "ທ່ານ ນາງ",
                    Id = x.id,
                    Phone = x.Contact.Phone,
                    RecordDate = x.RecordDate
                }).ToList();
            }
        }

        
    }
}
