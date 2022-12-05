using DFM.Shared.Entities;
using HttpClientService;
using System.Text.Json;

namespace DFM.Frontend.Pages.Common
{
    public partial class Recipient
    {
        private string searchString = "";
       
        //private EmployeeModel? employee;
        string? token = "";
        private bool FilterFunc(RoleTreeModel element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.Role.Display.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Role.Display.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Employee.Name.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Employee.Name.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Employee.FamilyName.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.Employee.FamilyName.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        private IEnumerable<string> MaxCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 1000 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 1000 ອັກສອນ";
        }
        private IEnumerable<string> MediumCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 500 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 500 ອັກສອນ";
        }
        //protected override async Task OnInitializedAsync()
        //{
        //    employee = await storageHelper.GetEmployeeProfileAsync();
        //    token = await accessToken.GetTokenAsync();
        //    string url = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID!}";
        //    //string url = $"{endpoint.API}/api/v1/Organization/GetItem/{employee.OrganizationID!}/{RoleId}";
        //    var result = await httpService.Get<IEnumerable<RoleTreeModel>>(url, new AuthorizeHeader("bearer", token));
        //    if (result.Success)
        //    {
        //        recipients = result.Response;
        //    }
            


        //    Console.WriteLine($"{DateTime.Now} Recipient Load");
        //}
        
    }
}
