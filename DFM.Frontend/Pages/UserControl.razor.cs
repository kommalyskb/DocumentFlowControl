using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages
{
    public partial class UserControl
    {
        string? editBreadcrumbText = "";
        readonly int delayTime = 500;
        private EmployeeModel? employee;
        string? token = "";
        protected override void OnInitialized()
        {
            formMode = FormMode.List;

            base.OnInitialized();

        }
        void onCreateButtonClick()
        {
            formMode = FormMode.Create;
        }
        void onePreviousButtonClick()
        {
            formMode = FormMode.List;
        }

        async Task onDeleteButtonClick()
        {
            bool? result = await delBox!.Show();
        }

        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onRowClick(EmployeeDto item)
        {
            // Row click
            formMode = FormMode.View;

        }

        async Task onSaveClickAsync()
        {
            onProcessing = true;
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            string url = $"{endpoint.API}/api/v1/Employee/SaveItem";
            string token = await accessToken.GetTokenAsync();
            // Send request for save document
            var result = await httpService.Post<EmployeeModel, CommonResponse>(url, employeeModel!, new AuthorizeHeader("bearer", token));

            // Open dialog success message or make small progress bar on top-corner

            onProcessing = false;

            Console.WriteLine($"-------------------------------");
            Console.WriteLine($"Result: {await result.HttpResponseMessage.Content.ReadAsStringAsync()}");
            Console.WriteLine($"-------------------------------");

            if (result.Success)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
            }
            else
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
            }

            await Task.Delay(delayTime);
            disposedObj();
            formMode = FormMode.List;
        }
        async Task onDeleteClickAsync()
        {
            formMode = FormMode.List;
        }
        void disposedObj()
        {
            employeeModel = new();
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
        void onMessageAlert(string message)
        {
            AlertMessage(message, Defaults.Classes.Position.BottomRight, Severity.Error);
        }
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
    }
}
