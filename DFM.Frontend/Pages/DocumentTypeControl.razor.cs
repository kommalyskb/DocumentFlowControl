using DFM.Shared.Common;
using DFM.Shared.Entities;
using HttpClientService;
using MudBlazor;

namespace DFM.Frontend.Pages
{
    public partial class DocumentTypeControl
    {
        readonly int delayTime = 500;
        private EmployeeModel? employee;

        protected override void OnInitialized()
        {
            formMode = FormMode.List;

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

                    string url = $"{endpoint.API}/api/v1/DocumentType/RemoveItem/{dataTypeModel!.id}";
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
        void onRowClick(DataTypeModel item)
        {
            // Row click
            formMode = FormMode.View;
            dataTypeModel = item;

        }

        async Task onSaveClickAsync()
        {
            onProcessing = true;
            string token = await accessToken.GetTokenAsync();
            await InvokeAsync(StateHasChanged);
            
            httpService.MediaType = MediaType.JSON;


            if (string.IsNullOrWhiteSpace(dataTypeModel.id))
            {
                if (employee == null)
                {
                    employee = await storageHelper.GetEmployeeProfileAsync();
                }
                dataTypeModel.OrganizationID = employee.OrganizationID;
                // New folder
                string url = $"{endpoint.API}/api/v1/DocumentType/NewItem";
                var result = await httpService.Post<DataTypeModel, CommonResponseId>(url, dataTypeModel, new AuthorizeHeader("bearer", token));

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
                string url = $"{endpoint.API}/api/v1/DocumentType/UpdateItem";
                var result = await httpService.Post<DataTypeModel, CommonResponseId>(url, dataTypeModel, new AuthorizeHeader("bearer", token));

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
            dataTypeModel = new();
        }

        void onTabChangeEvent(string roleId)
        {
            this.roleId = roleId;
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
    }
}
