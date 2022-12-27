using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Resources;
using HttpClientService;
using MudBlazor;
using System;

namespace DFM.Frontend.Pages
{
    public partial class FolderControl
    {
        string? oldLink = "";
        readonly int delayTime = 500;
        private EmployeeModel? employee;
        protected override void OnInitialized()
        {
            formMode = FormMode.List;
            oldLink = Link!;

            base.OnInitialized();

        }
        void onCreateButtonClick()
        {
            disposedObj();
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
                    onProcessing = true;
                    string token = await accessToken.GetTokenAsync();

                    string url = $"{endpoint.API}/api/v1/Folder/RemoveItem/{folderModel!.id}";
                    var result = await httpService.Get<CommonResponse>(url, new AuthorizeHeader("bearer", token));

                    if (result.Success)
                    {
                        AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                    }
                    else
                    {
                        AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    }


                    onProcessing = false;

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
        void onRowClick(FolderModel item)
        {
            // Row click
            formMode = FormMode.View;
            folderModel = item;

        }
        
        async Task onSaveClickAsync()
        {
            onProcessing = true;
            string token = await accessToken.GetTokenAsync();
            await InvokeAsync(StateHasChanged);
            if (Link == "inbound")
            {
                folderModel!.InboxType = InboxType.Inbound;

            }
            else
            {
                folderModel!.InboxType = InboxType.Outbound;
            }
            httpService.MediaType = MediaType.JSON;
            if (string.IsNullOrWhiteSpace(folderModel.id))
            {
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                };
                folderModel.OrganizationID = employee.OrganizationID;
                // New folder
                string url = $"{endpoint.API}/api/v1/Folder/NewItem";
                var result = await httpService.Post<FolderModel, CommonResponseId>(url, folderModel, new AuthorizeHeader("bearer", token));

                if (result.Success)
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                }
                else
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                }
            }
            else
            {
                // Update folder
                string url = $"{endpoint.API}/api/v1/Folder/UpdateItem";
                var result = await httpService.Post<FolderModel, CommonResponseId>(url, folderModel, new AuthorizeHeader("bearer", token));

                if (result.Success)
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);
                }
                else
                {
                    AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ", Defaults.Classes.Position.BottomRight, Severity.Error);
                }
            }

            onProcessing = false;

            await Task.Delay(delayTime);

            disposedObj();
            formMode = FormMode.List;
        }
        void disposedObj()
        {
            folderModel = new();
        }

       
        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
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
        void onTabChangeEvent(string roleId)
        {
            this.roleId = roleId;
        }
        protected override void OnParametersSet()
        {
            if (oldLink != Link)
            {
                formMode = FormMode.List;
                oldLink = Link!;
            }
            base.OnParametersSet();
        }
    }
}
