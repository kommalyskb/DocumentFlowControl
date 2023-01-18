using DFM.Shared.Entities;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserView
    {
        EmployeeModel? Employee = new();
        protected override async Task OnInitializedAsync()
        {
            Employee = await storageHelper.GetEmployeeProfileAsync();
        }
    }
}
