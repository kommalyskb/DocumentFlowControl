﻿using DFM.Shared.Common;
using DFM.Shared.Entities;
using DFM.Shared.Helper;
using HttpClientService;
using MudBlazor;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages
{
    public partial class SecurityLevelControl
    {
        readonly int delayTime = 500;
        private EmployeeModel? employee;
        string? token;
        protected override async Task OnInitializedAsync()
        {
            var rules = await storageHelper.GetRuleMenuAsync();
            if (!ValidateRule.isInRole(rules, "/pages/security"))
            {
                nav.NavigateTo("/pages/unauthorized");
            }

            formMode = FormMode.List;
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
            try
            {
                bool? isDelete = await delBox!.Show();
                if (isDelete.HasValue)
                {
                    if (isDelete.Value)
                    {
                        #region Validate Token
                        var getTokenState = await tokenState.ValidateToken();
                        if (!getTokenState)
                            nav.NavigateTo("/authorize");
                        #endregion
                        onProcessing = true;
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            token = await accessToken.GetTokenAsync();
                        }

                        string url = $"{endpoint.API}/api/v1/SecurityLevel/RemoveItem/{documentSecurityModel!.id}";
                        var result = await httpService.Get<CommonResponse>(url, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

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
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            
        }

        void onEditButtonClick()
        {
            formMode = FormMode.Edit;
        }
        void onRowClick(DocumentSecurityModel item)
        {
            // Row click
            formMode = FormMode.View;
            documentSecurityModel = item;

        }

        async Task onSaveClickAsync()
        {
            try
            {
                #region Validate Token
                var getTokenState = await tokenState.ValidateToken();
                if (!getTokenState)
                    nav.NavigateTo("/authorize");
                #endregion
                onProcessing = true;
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }
                await InvokeAsync(StateHasChanged);

                httpService.MediaType = MediaType.JSON;
                if (string.IsNullOrWhiteSpace(documentSecurityModel.Level))
                {
                    AlertMessage("ກະລຸນາ ປ້ອນຂໍ້ມູນໃຫ້ຄົບຖ້ວນ", Defaults.Classes.Position.BottomRight, Severity.Error);
                    onProcessing = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(documentSecurityModel.id))
                {
                    if (employee == null)
                    {
                        employee = await storageHelper.GetEmployeeProfileAsync();
                    }
                    documentSecurityModel.OrganizationID = employee.OrganizationID;
                    // New folder
                    string url = $"{endpoint.API}/api/v1/SecurityLevel/NewItem";
                    var result = await httpService.Post<DocumentSecurityModel, CommonResponseId>(url, documentSecurityModel, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

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
                    string url = $"{endpoint.API}/api/v1/SecurityLevel/UpdateItem";
                    var result = await httpService.Post<DocumentSecurityModel, CommonResponseId>(url, documentSecurityModel, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);

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
            catch (Exception)
            {
                AlertMessage("ທຸລະກຳຂອງທ່ານ ຜິດພາດ, (INTERNAL_SERVER_ERROR)", Defaults.Classes.Position.BottomRight, Severity.Error);
            }
            
        }
        void disposedObj()
        {
            documentSecurityModel = new();
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
