using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Resources;
using HttpClientService;
using MudBlazor;
using StackExchange.Redis;

namespace DFM.Frontend.Pages
{
    public partial class Organization
    {
        string? editBreadcrumbText = "";
        readonly int delayTime = 500;
        private EmployeeModel? employee;
        string? token = "";
        bool onProcessing = false;

        protected override void OnInitialized()
        {
            formMode = FormMode.List;

            base.OnInitialized();

        }
        void onePreviousButtonClick()
        {
            formMode = FormMode.List;
        }

        async Task onSaveClickAsync()
        {
            if (string.IsNullOrWhiteSpace(roleTreeModel!.Publisher) || string.IsNullOrWhiteSpace(roleTreeModel!.Role.Display.Local) ||
                string.IsNullOrWhiteSpace(roleTreeModel!.Role.Display.Eng) || string.IsNullOrWhiteSpace(roleTreeModel!.Employee.UserID))
            {
                AlertMessage("ກະລຸນາ ປ້ອນຂໍ້ມູນໃຫ້ຄົບຖ້ວນ", Defaults.Classes.Position.BottomRight, Severity.Error);
                return;
            }
            onProcessing = true;
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            string url = $"{endpoint.API}/api/v1/Organization/SavePosition/{employee.OrganizationID}";
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }

            // Send request for save document
            roleTreeModel!.RoleType = roleTreeModel.Role.RoleType;
            var result = await httpService.Post<RoleTreeModel, CommonResponse>(url, roleTreeModel!, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

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
        async Task onDeleteButtonClick()
        {
            var delResult = await delBox!.Show();
            if (delResult.HasValue)
            {
                if (delResult.Value)
                {
                    onProcessing = true;
                    if (employee == null)
                    {
                        employee = await storageHelper.GetEmployeeProfileAsync();
                    }
                    string url = $"{endpoint.API}/api/v1/Organization/RemovePosition/{employee.OrganizationID}/{roleTreeModel!.Role.RoleID}";
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        token = await accessToken.GetTokenAsync();
                    }
                    var result = await httpService.Get<CommonResponse>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
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
            }
            
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
        
        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onCreateButtonClick()
        {
            formMode = FormMode.Create;
            // Dispose object
            disposedObj();
        }

        void disposedObj()
        {
            roleTreeModel = new();
        }
        void onItemChangeEvent(TreeItemData item)
        {
            if (item != null)
            {
                formMode = FormMode.View;
                editBreadcrumbText = item.Content!.Role.Display.Local;
                roleTreeModel = item.Content;
                orgId = item.OrganizationID;
            }
           
        }

        void onMessageAlert(string message)
        {
            AlertMessage(message, Defaults.Classes.Position.BottomRight, Severity.Info);
        }
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }
    }
    
}
