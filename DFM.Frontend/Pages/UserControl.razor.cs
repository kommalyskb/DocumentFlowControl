using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using Google.Protobuf.WellKnownTypes;
using HttpClientService;
using MudBlazor;
using System;

namespace DFM.Frontend.Pages
{
    public partial class UserControl
    {
        long maxFileSize = 1024 * 1024 * 25; // 5 MB or whatever, don't just use max int
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
            bool? isDelete = await delBox!.Show();
            if (isDelete.HasValue)
            {
                if (isDelete.Value)
                {
                    // Delete button had fire
                    onProcessing = true;
                    if (employee == null)
                    {
                        employee = await storageHelper.GetEmployeeProfileAsync();
                    }
                    await InvokeAsync(StateHasChanged);
                    string url = $"{endpoint.API}/api/v1/Employee/RemoveItem/{employeeModel!.id}";
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        token = await accessToken.GetTokenAsync();
                    }
                    // Send request for save document
                    var result = await httpService.Get<CommonResponse, CommonResponse>(url, new AuthorizeHeader("bearer", token));
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

        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onRowClick(EmployeeModel item)
        {
            // Set employee model
            employeeModel = item;
            // Row click
            formMode = FormMode.Edit;


        }

        private async Task openResetPasswordBox()
        {
            bool? isOpen = await mbox!.Show();
            if (isOpen.HasValue)
            {
                if (isOpen.Value)
                {
                    onProcessing = true;
                    if (employee == null)
                    {
                        employee = await storageHelper.GetEmployeeProfileAsync();
                    }

                    string url = $"{endpoint.API}/api/v1/Employee/ResetPassword";
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        token = await accessToken.GetTokenAsync();
                    }

                    var result = await httpService.Post<EmployeeModel, CommonResponseId>(url, employeeModel!, new AuthorizeHeader("bearer", token));


                    onProcessing = false;

                    Console.WriteLine($"-------------------------------");
                    Console.WriteLine($"Result: {await result.HttpResponseMessage.Content.ReadAsStringAsync()}");
                    Console.WriteLine($"-------------------------------");

                    if (result.Success)
                    {
                        employeeModel!.Password = result.Response.Id;
                        AlertMessage("ປ່ຽນລະຫັດຜ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                    }
                    else
                    {
                        AlertMessage("ປ່ຽນລະຫັດຜ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    }

                    await Task.Delay(delayTime);

                }
            }
        }
        private IEnumerable<string> PasswordCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 6 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 12 ອັກສອນ";
        }
        async Task onSaveClickAsync()
        {
            onProcessing = true;
            if (employee == null)
            {
                employee = await storageHelper.GetEmployeeProfileAsync();
            }
            if (string.IsNullOrWhiteSpace(employeeModel!.Name.Local) || string.IsNullOrWhiteSpace(employeeModel!.Name.Eng) ||
               string.IsNullOrWhiteSpace(employeeModel!.FamilyName.Local) || string.IsNullOrWhiteSpace(employeeModel!.FamilyName.Eng))
            {
                AlertMessage("ກະລຸນາ ປ້ອນຊື່ ພະນັກງານ ທີ່ຈະເປັນ ຜູ້ດູແລລະບົບ", Defaults.Classes.Position.BottomRight, Severity.Error);
                onProcessing = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(employeeModel!.Username) || string.IsNullOrWhiteSpace(employeeModel!.Password) ||
                string.IsNullOrWhiteSpace(employeeModel!.Contact.Email) || string.IsNullOrWhiteSpace(employeeModel!.Contact.Phone))
            {
                AlertMessage("ກະລຸນາ ກວດເບີ່ງວ່າຂໍ້ມູນ Username, Password, Email, Phone ປ້ອນແລ້ວບໍ່", Defaults.Classes.Position.BottomRight, Severity.Error);
                onProcessing = false;
                return;
            }
            string isNotify = "no";
            if (notify)
            {
                isNotify = "yes";
            }
            string url = $"{endpoint.API}/api/v1/Employee/SaveItem?notify={isNotify}";
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await accessToken.GetTokenAsync();
            }


            // Upload file via Minio SDK
            await manageFileToServer();

            employeeModel!.OrganizationID = employee.OrganizationID;
            employeeModel!.ProfileImage = attachment!.Info;
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
            AlertMessage(message, Defaults.Classes.Position.BottomRight, Severity.Info);
        }
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }

        private async Task manageFileToServer()
        {
            if (attachment!.File != null)
            {
                // Upload new file
                using Stream readStream = attachment.File!.OpenReadStream(maxFileSize);

                var buf = new byte[readStream.Length];

                using MemoryStream ms = new MemoryStream(buf);

                await readStream.CopyToAsync(ms);
                var buffer = ms.ToArray();

                await minio.PutObject(attachment.Info.Bucket!, attachment.Info.FileName!, buffer);
            }


        }

    }
}
